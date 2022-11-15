using MessagePack;
using Newtonsoft.Json;
using PestelLib.ClientConfig;
using PestelLib.ServerClientUtils;
using PestelLib.Utils;
using S;
using UnityDI;
using UnityEngine;

public class LeaderboardApiTest : MonoBehaviour {
    private RequestQueue _requestQueue;

    [SerializeField] private string leaderboardType = "HonorPoints";
    [SerializeField] private int playerScore = 123;
    [SerializeField] private string playerName = "MyPlayerName";

    [SerializeField] private int globalTopFrom = 1;
    [SerializeField] private int globalTopTo = 3;

    [SerializeField] private int getChunkLeagueIndex = 0;
    [SerializeField] private bool useSpecifiedChunkLeague = false;

    void Start () {
	    //отдельные скрипты из PestelLib получают друг на друга ссылки через этот контейнер.
	    //для того, что бы они могли это сделать, нужно сконфигурировать этот контейнер, указав ему откуда какие классы брать
	    var container = ContainerHolder.Container;

	    //дальше указываем, откуда взять вещи нужные для полноценной работы CommandProcessor и RequestQueue
	    container.RegisterUnityScriptableObject<Config>(); //в конфиге задается URL сервера, конфиг кладется в Resources/Singleton в виде ScriptableObject'а
	    container.RegisterUnitySingleton<UpdateProvider>(null, true); //это нужно для работы RequestQueue.

        //Через RequestQueue мы будем получать сохранённый на сервере стейт игрока и делать другие запросы
        container.RegisterSingleton<RequestQueue>();
	    
	    //получаем экземпляр RequestQueue. container позаботится о его создании и создании необходимых для его работы зависимостей.
	    _requestQueue = container.Resolve<RequestQueue>();
    }

    /*
     * Получение глобального топа игроков, первые LeaderboardGetRankTop.Amount записей для таблицы с именем LeaderboardGetRankTop.Type
     * Каждая запись обязательно содержит:
     * -позицию
     * -имя игрока
     * -PlayerID этого игрока
     * -счёт игрока;
     * Может так же содержать FacebookId, если он был передан при регистрации рекорда
     */
    [ContextMenu("Get global top")]
    public void GetGlobalTop() {
        _requestQueue.SendRequest("LeaderboardGetRankTop",
            new Request
            {
                LeaderboardGetRankTop = new LeaderboardGetRankTop
                {
                    Amount = 100,
                    Type = leaderboardType
                }
            },
            (response, collection) =>
            {
                var serializedResponse = collection.Data;
                var resp = MessagePackSerializer.Deserialize<LeaderboardGetRankTopResponse>(serializedResponse);
                Debug.LogFormat("LeaderboardGetRankTop: {0}", JsonConvert.SerializeObject(resp, Formatting.Indented));
            }
        );
    }

    /*
     * Аналогично GetGlobalTop, но позволяет указать диапазон - например запросить места с 100 по 200.
     * Это может быть полезно для постраничного отображения рекордов, либо для того, что бы отобразить игроков, у которых счет близок ко счету игрока.
     */
    [ContextMenu("Get global top segment")]
    public void GetGlobalTopPart()
    {
        _requestQueue.SendRequest("LeaderboardGetRankTop",
            new Request
            {
                LeaderboardGetRankTop = new LeaderboardGetRankTop
                {
                    From = globalTopFrom,
                    To = globalTopTo,
                    Type = leaderboardType
                }
            },
            (response, collection) =>
            {
                var serializedResponse = collection.Data;
                var resp = MessagePackSerializer.Deserialize<LeaderboardGetRankTopResponse>(serializedResponse);
                Debug.LogFormat("LeaderboardGetRankTop: {0}", JsonConvert.SerializeObject(resp, Formatting.Indented));
            }
        );
    }

    /*
     * Получение позиции игрока с текущим PlayerID.
     * Обычно игрок не входит в первый Top-100 (ну или Top-300 или сколько будет задано в LeaderboardGetRankTop.Amount)
     * Поэтому для получения его позиции, которая может быть например 15384 есть отдельный запрос
     * Обратите внимание - тип ответа такой же, как у запроса на весь топ: LeaderboardGetRankTopResponse
     */
    [ContextMenu("Get player position in global top")]
    public void GetPlayerPositionInGlobalTop()
    {
        _requestQueue.SendRequest("LeaderboardGetRank", 
            new Request
            {
                LeaderboardGetRank = new LeaderboardGetRank()
                {
                    Type = leaderboardType
                }
            }, 
            (response, collection) =>
            {
                var serializedResponse = collection.Data;
                var resp = MessagePackSerializer.Deserialize<LeaderboardGetRankTopResponse>(serializedResponse);
                Debug.LogFormat("LeaderboardGetRank: {0}", JsonConvert.SerializeObject(resp, Formatting.Indented));
            }
        );
    }

    /*
     * Отправить в лидерборд очки игрока
     */
    [ContextMenu("Set player score")]
    public void SetPlayerScore()
    {
        _requestQueue.SendRequest("LeaderboardRegisterRecord", new Request
        {
            LeaderboardRegisterRecord = new LeaderboardRegisterRecord()
            {
                Type = leaderboardType,
                Add = false, //если поставить true, то значение очков будет выставлятся не в переданный Score, а добавляться к тому, что уже есть в лидерборде
                FacebookId = "",
                Name = playerName,
                Score = playerScore
            }
        }, (response, collection) =>
        {
            Debug.Log("SetPlayerScore: done");
        });
    }

    /*
     * Получить "локальный" лидерборд - подробнее см здесь:
     * https://docs.google.com/document/d/1gPVHADcVJUabEioCgCHn4A-iuuy_zlnP4eQGuYhvnUE/edit#heading=h.2qdjfixvxlbe
     * Всегда содержит в т.ч. и результат игрока
     */
    [ContextMenu("Get chunk top")]
    public void GetChunkTop()
    {
        if (useSpecifiedChunkLeague) { }

        _requestQueue.SendRequest("LeaderboardGetRankTopChunk", new Request
        {
            LeaderboardGetRankTopChunk = new LeaderboardGetRankTopChunk
            {
                Type = leaderboardType,
                LeagueIndex = getChunkLeagueIndex, //можно не задавать, тогда лига определиться по текущему счету игрока
                UseLeagueIndex = useSpecifiedChunkLeague //брать лигу по счету игрока, либо использовать поле LeagueIndex запроса
            }
        }, (response, collection) =>
        {
            var resp = MessagePackSerializer.Deserialize<LeaderboardGetRankTopChunkResponse>(collection.Data);
            Debug.LogFormat("GetChunkTop: {0}", JsonConvert.SerializeObject(resp, Formatting.Indented));
        });
    }

    /*
     * Получение информации о текущем сезоне - индекс и полный строковый идентификатор
     * Индекс обновляется через каждые LeaderboardConfig.SeasonDuration
     */
    [ContextMenu("Get current season info")]
    public void GetCurrentSeasonInfo()
    {
        _requestQueue.SendRequest("LeaderboardGetRankTop",
            new Request
            {
                LeaderboardGetSeasonInfoRequest = new LeaderboardGetSeasonInfoRequest()
            },
            (response, collection) =>
            {
                var serializedResponse = collection.Data;
                var resp = MessagePackSerializer.Deserialize<LeaderboardGetSeasonInfoResponse>(serializedResponse);
                Debug.LogFormat("LeaderboardGetSeasonInfoResponse: {0}", JsonConvert.SerializeObject(resp, Formatting.Indented));
            }
        );
    }
}
