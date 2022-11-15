using System;

public interface IQualitySettingsManager
{
    int CurrentQuality { get; }

    event Action<int> OnChangeQuality;

    void SetQualityByUserChoice(int qualityLevel);
}
