using System;
using System.Collections.Generic;

public interface IChoiceEffect {
    bool HasChoice(CardChoice choice);
    string GetEffectChoicePrompt(CardChoice choice);
    List<string> EffectChoices(CardChoice choice);
    void ApplyEffect(int id);
}
