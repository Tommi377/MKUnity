using System;
using System.Collections.Generic;

public interface IChoiceEffect {
    bool HasChoice(CardChoice choice);
    string GetEffectChoicePrompt(CardChoice choice);
    List<(string, Action)> EffectChoices(CardChoice choice);
}
