using System;
using System.Threading.Tasks;

namespace ClansServerLib
{
    interface IClansDbPrivate
    {
        // на бэкэнде удален юзер. необходимо подчистить следы его пристутствия в БД кланов:
        // * удалить все заявки
        // * выйти из клана
        // * если кланлидер, передать лидерство рандому (если число участников > 1)
        Task UserDeletedPrivate(Guid userId);
        // на бэкэнде забанен юзер
        // * передать лидерство клана члену с наибольшим внутриклановым рейтингом
        Task UserBannedPrivate(Guid userId);
    }
}
