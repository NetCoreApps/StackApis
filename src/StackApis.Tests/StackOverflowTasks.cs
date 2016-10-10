using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using ServiceStack;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using ServiceStack.Text;
using StackApis.ServiceModel;
using StackApis.ServiceModel.Types;

namespace StackApis.Tests
{
    [TestFixture]
    public class StackOverflowTasks
    {
        private IDbConnectionFactory dbFactory;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var dbPath = "~/App_Data/db.sqlite".MapProjectPath();
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
            dbFactory = new OrmLiteConnectionFactory(dbPath, SqliteDialect.Provider);
        }

        [Test]
        public void Import_from_StackOverflow()
        {
            int numberOfPages = 100;
            int pageSize = 100;
            var dbQuestions = new List<Question>();
            var dbAnswers = new List<Answer>();
            try
            {
                for (int i = 1; i < numberOfPages + 1; i++)
                {
                    //Throttle queries
                    Thread.Sleep(100);
                    //StackOverflow API always returns gzipped response and .NET Core HttpWebRequest doesn't support auto uncompressing yet so need to decompress manually
                    var bytes = $"https://api.stackexchange.com/2.2/questions?page={i}&pagesize={pageSize}&site=stackoverflow&tagged=servicestack".GetBytesFromUrl();
                    var json = bytes.GUnzip();

                    QuestionsResponse qResponse;
                    using (new ConfigScope())
                    {
                        qResponse = json.FromJson<QuestionsResponse>();
                        dbQuestions.AddRange(qResponse.Items.Select(q => q.ConvertTo<Question>()));
                    }

                    var acceptedAnswers =
                        qResponse.Items
                        .Where(x => x.AcceptedAnswerId != null)
                        .Select(x => x.AcceptedAnswerId).ToList();

                    var answersList = acceptedAnswers.Join(";");
                    bytes = $"https://api.stackexchange.com/2.2/answers/{answersList}?sort=activity&site=stackoverflow".GetBytesFromUrl();
                    json = bytes.GUnzip();

                    using (new ConfigScope())
                    {
                        var aResponse = JsonSerializer.DeserializeFromString<AnswersResponse>(json);
                        dbAnswers.AddRange(aResponse.Items.Select(a => a.ConvertTo<Answer>()));
                    }
                }
            }
            catch (Exception ex)
            {
                //ignore
                ex.Message.Print();
            }

            //Filter duplicates
            dbQuestions = dbQuestions.GroupBy(q => q.QuestionId).Select(q => q.First()).ToList();
            dbAnswers = dbAnswers.GroupBy(a => a.AnswerId).Select(a => a.First()).ToList();
            var questionTags = dbQuestions.SelectMany(q =>
                q.Tags.Select(t => new QuestionTag { QuestionId = q.QuestionId, Tag = t }));

            using (var db = dbFactory.OpenDbConnection())
            {
                db.DropAndCreateTable<Question>();
                db.DropAndCreateTable<Answer>();
                db.DropAndCreateTable<QuestionTag>();

                db.InsertAll(dbQuestions);
                db.InsertAll(dbAnswers);
                db.InsertAll(questionTags);
            }
        }

        [Test]
        public void Test_Import()
        {
            using (var db = dbFactory.OpenDbConnection())
            {
                var numberOfQuestions = db.Count<Question>();
                var numberOfAnswers = db.Count<Answer>();
                Assert.That(numberOfQuestions > 0);
                Assert.That(numberOfAnswers > 0);
            }
        }
    }

    public class ConfigScope : IDisposable
    {
        private readonly WriteComplexTypeDelegate holdQsStrategy;
        private readonly JsConfigScope scope;

        public ConfigScope()
        {
            scope = JsConfig.With(
                dateHandler: DateHandler.UnixTime,
                propertyConvention: PropertyConvention.Lenient,
                emitLowercaseUnderscoreNames: true,
                emitCamelCaseNames: false);

            holdQsStrategy = QueryStringSerializer.ComplexTypeStrategy;
            QueryStringSerializer.ComplexTypeStrategy = QueryStringStrategy.FormUrlEncoded;
        }

        public void Dispose()
        {
            QueryStringSerializer.ComplexTypeStrategy = holdQsStrategy;
            scope.Dispose();
        }
    }

}
