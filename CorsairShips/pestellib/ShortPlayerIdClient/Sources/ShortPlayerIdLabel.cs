using System;
using PestelLib.ServerClientUtils;
using UnityEngine;
using UnityDI;
using UnityEngine.UI;

namespace ShortPlayerIdClient
{
    /*
     * Дополнительные короткие ID игроков.
     * Использование:
     * 1. Подключить через PackageManager пакет ShortPlayerIdClient
     * 2. Повесить на лейбл UI.Text скрипт ShortPlayerIdLabel,
     * он запросит у сервера короткий ID и назначет его полю _text после загрузки
     *
     * Либо запросить ID самостоятельно через такой вызов:
     * _requestQueue.GetShortPlayerId(_requestQueue.PlayerId, (id) => Debug.Log($"received id: {id}"));
     *
     * можно передавать не только свой, но и чужие playerId
     *
     * В админке пока что короткие ID поддерживаются на страницах ProfileViewer и DeleteUser
     *
     * По умолчанию, если связи с бэкэндом нет, то в поле запишется полный PlayerId, если такое поведение
     * нежелательно выставите _dontFallbackToPlayerId = true
     */
    public class ShortPlayerIdLabel : MonoBehaviour
    {
        [SerializeField] private Text _text;
        [SerializeField] private bool _dontFallbackToPlayerId;

        [Dependency] private RequestQueue _requestQueue;

        void Start()
        {
            ContainerHolder.Container.BuildUp(this);

            if (_text != null)
            {
                var shortId = _requestQueue.ShortId;
                var playerId = _requestQueue.PlayerId;
                if (shortId > 0)
                    _text.text = shortId.ToString();
                else if (!_dontFallbackToPlayerId && playerId != Guid.Empty)
                    _text.text = playerId.ToString();

                // случай когда связи с бэкэндом нет и ShortId не успел прокешироваться в локальной базе
                // пробуем запросить его, в случае успеха меняем поле в котором пока будет PlayerId на то что придет (это очень маловероятно, т.к. скорее всего бэк лежит)
                if (shortId == 0)
                {
                    _requestQueue.GetShortPlayerId(_requestQueue.PlayerId, i => _text.text = i.ToString());
                }
            }
        }
    }
}