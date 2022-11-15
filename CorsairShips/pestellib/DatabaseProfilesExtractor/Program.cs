using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using PestelLib.SharedLogic.Modules;
using PestelLib.UniversalSerializer;
using S;

namespace DatabaseProfilesExtractor
{
    public class Program
    {
        [BsonIgnoreExtraElements]
        class RawData
        {
            [BsonId]
            public string Id;
            [BsonElement("data")]
            public byte[] Data;
            [BsonElement("time")]
            public DateTime Time;
            [BsonElement("size")]
            public int Size;
        }

        static void Main(string[] args)
        {
            //тут нужно заменить содержимое функции таким образом, чтобы она забирала нужные вам данные из профиля и выводила их, или сохраняла в файл, или в БД
            void ExtractGameSpecificDataFromUserProfile(UserProfile state, string userId)
            {
                /*
                if (state.ModulesDict.ContainsKey(nameof(MoneyModule)))
                {
                    var moneyModuleState =
                        Serializer.Deserialize<MoneyModuleState>(state.ModulesDict[nameof(MoneyModule)]);
                    var hard = moneyModuleState.CurrencyStates[(int) CurrencyType.HARD];
                    Console.WriteLine(userId + "\t" + hard.Amount);
                }
                */
            }

            var c = new MongoClient("mongodb://localhost");
            var db = c.GetDatabase("database_android");
            var statesCollection = db.GetCollection<RawData>("UserStates");

            foreach (var rawData in statesCollection.AsQueryable())
            {
                var state = Serializer.Deserialize<UserProfile>(rawData.Data);
                ExtractGameSpecificDataFromUserProfile(state, rawData.Id);
            }
        }
    }
}
