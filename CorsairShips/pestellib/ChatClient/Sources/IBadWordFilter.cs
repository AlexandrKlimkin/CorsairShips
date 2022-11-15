using System.Collections.Generic;
using PestelLib.ChatCommon;
using UnityEngine;

namespace PestelLib.ChatClient
{
    public class FilterReport : Dictionary<BanReason, int>
    {
    }

    public interface IBadWordFilter
    {
        /// <summary>
        /// Определяет наличие мата в сообщении и фильтрует сообщение
        /// </summary>
        /// <param name="message">исходное сообщение</param>
        /// <param name="language">язык сообщения</param>
        /// <returns>количество отфильтрованных сущностей то типам</returns>
        FilterReport Filter(ref string message, SystemLanguage language);

        FilterReport Filter(ref string message);
    }
}
