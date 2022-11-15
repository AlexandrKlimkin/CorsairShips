﻿using System;
using S;

namespace PestelLib.SharedLogic.Modules
{
    [Serializable]
    public class QuestDef
    {
        public string Id;
        public string RevenueId;

        //сколько раз нужно сделать действие типа QuestType для выполнения квеста
        public int Amount;
        
        public string QuestType; //тип действий, требуемый для прогресса в квесте
        public string QuestClass; //тип квеста. Могут отображаться на разных вкладках. Встроенные типы: Regular, Tutorial, Achievement

        public bool AddToDefaultState; //добавлять ли этот квест при создании пустого профиля пользователя

        //сколько квест висит в "кулдауне" после добавления
        public int CooldownTime;

        //минимальный уровень игрока, требуемый для этого квеста
        public int MinLevel;

        //максимальный уровень игрока, после которого этот квест не будет выдаваться игроку среди других случайных квестов
        public int MaxLevel;

        //если это поле не пустое, то после забирания награды за данный квест будет добавлен квест с заданным ид. Иначе - случайный квест, подходящий по уровню.
        public string NextLevelQuestId;

        //некоторые квесты не должны удалятся после прохождения. Например - учебные квесты (иначе могут быть выданы повторно), или ачивки последнего уровня. Или же Weekly квесты.
        public bool KeepInState;

        //некоторые квесты не должны скрываться никогда, даже после того как забрали награду. Например - ачивки последнего уровня.
        public bool AlwaysVisible;
        
        //некоторые квесты не могут быть выданы случайно. К примеру, ачивки.
        public bool CanBeGivenByRandom;
        
        //дополнительные поля, используемые для отображения в UI
        public string Name;
        public string Description;
        public string Icon;

        /*
         * В ачивках 2го, 3го уровней хочется писать кол-во действий с учетом требований к предыдущим ачивкам.
         * Т.о. если ачивка первого уровня: сделать суммарно 10 фрагов
         * второго: сделать суммарно 100 фрагов
         * и мы хотим что бы при получении ачивки второго номера счетчик был 10/100 нужно задать в ачивке второго уровня AmountOffset=10; Amount=90
         */
        public int AmountOffset;
        
        //сколько "звезд" отобразить в многоуровневой ачивке. Обычно 0, 1, 2
        public int AchievementLevel; 

        //если есть доступный учебный квест, он будет добавлен в первую очередь при добавлении "случайного" квеста
        public bool IsTutorial;

        //для Weekly эвентов - в каждую неделю выдаются квесты с одним значением QuestGroupIndex
        public int QuestGroupIndex;
    }
}