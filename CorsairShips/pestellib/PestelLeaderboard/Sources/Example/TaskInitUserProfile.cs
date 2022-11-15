using System;
using System.Collections;
using System.Collections.Generic;
using MessagePack;
using PestelLib.ServerClientUtils;
using PestelLib.SharedLogic;
using PestelLib.SharedLogicBase;
using PestelLib.TaskQueueLib;
using S;
using UnityDI;
using UnityEngine;

namespace PestelLib.Leaderboard
{
    public class TaskInitUserProfile : Task {
        public override void Run()
        {
            //получаем экземпляр RequestQueue. container позаботится о его создании и создании необходимых для его работы зависимостей.
            var requestQueue = ContainerHolder.Container.Resolve<RequestQueue>();

            //создаем запрос на получение данных пользователя. Любой запрос на сервер имеет класс S.Request, отдельные запросы различаются по тому, 
            //какое поле в этом запросе заполнено. В данном случае заполнено только InitRequest, параметров у этого запроса нет
            var request = new Request
            {
                InitRequest = new InitRequest()
            };

            //отправляем запрос, результат придет в OnUserStateLoaded
            //здесь "InitData" - произвольное имя, используется сейчас только для статистики по запросам на сервере, так легче искать "медленный" тип запросов
            requestQueue.SendRequest("InitRequest", request, OnUserStateLoaded);
        }

        private void OnUserStateLoaded(Response response, DataCollection dataCollection)
        {
            var serializedUserState = dataCollection.Data;  //получаем из ответа сервера сериализованные данные игрока

            //эти данные нам непосредственно, они нужны для работы ШЛ.
            var userState = MessagePackSerializer.Deserialize<UserProfile>(serializedUserState);
            
            var assembly = typeof(UserProfile).Assembly;

            //помимо состояния игрока для работы ШЛ нужны разные игровые данные из дефов:
            var definitionsType = assembly.GetType("PestelLib.SharedLogic.Definitions");
            var sharedLogicType = assembly.GetType("PestelLib.SharedLogic.SharedLogicCore");

            Type[] sharedLogicConstructorArgTypes = { typeof(S.UserProfile), definitionsType };

            var slConstructor = sharedLogicType.GetConstructor(sharedLogicConstructorArgTypes);

            var defs = ContainerHolder.Container.Resolve(definitionsType);

            //создаём экземпляр ШЛ, он нужен для того что бы CommandProcessor вызываемый через 
            //SharedLogicCommand.<ModuleName>.<MethodName>(...) смог работать
            var sharedLogic = (ISharedLogic)slConstructor.Invoke(new System.Object[] { userState, defs });

            //экземпляр нужно зарегистрировать в контейнере, именно из контейнера его попытается получить CommandProcessor при выполнении команд ШЛ
            ContainerHolder.Container.RegisterInstance<ISharedLogic>(sharedLogic);
            OnComplete(this);
        }
    }

}
