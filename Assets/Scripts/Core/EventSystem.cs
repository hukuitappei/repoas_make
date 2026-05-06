using System;
using System.Collections.Generic;

public class EventSystem
{
    private readonly List<EventData> _events;
    private readonly Random _random;

    public EventSystem()
    {
        _events = new List<EventData>();
        _random = new Random();
    }

    public void RegisterEvent(EventData eventData)
    {
        if (eventData != null)
        {
            _events.Add(eventData);
            _events.Sort((left, right) => right.priority.CompareTo(left.priority));
        }
    }

    public void ResolveTurnStartEvents(GameState state)
    {
        if (state == null)
        {
            return;
        }

        for (int i = 0; i < _events.Count; i++)
        {
            EventData eventData = _events[i];
            if (eventData == null || _random.NextDouble() > eventData.baseProbability)
            {
                continue;
            }

            if (!ArePrerequisitesMet(state, eventData.prerequisiteConditions))
            {
                continue;
            }

            ApplyEffects(state, eventData.effects);
        }
    }

    private bool ArePrerequisitesMet(GameState state, string[] conditions)
    {
        if (conditions == null || conditions.Length == 0)
        {
            return true;
        }

        for (int i = 0; i < conditions.Length; i++)
        {
            if (!IsConditionMet(state, conditions[i]))
            {
                return false;
            }
        }

        return true;
    }

    private bool IsConditionMet(GameState state, string condition)
    {
        if (string.IsNullOrEmpty(condition))
        {
            return true;
        }

        if (condition == "happiness_high")
        {
            return state.Happiness >= 80;
        }

        if (condition == "happiness_low")
        {
            return state.Happiness < 20;
        }

        if (condition == "happiness_crisis")
        {
            return state.Happiness < 20;
        }

        if (condition.StartsWith("research_completed:"))
        {
            string nodeId = condition.Substring("research_completed:".Length);
            return state.IsResearchNodeCompleted(nodeId);
        }

        return true;
    }

    public void ApplyChoice(GameState state, EventChoice choice)
    {
        if (state == null || choice == null)
        {
            return;
        }

        if (choice.requiredFunds > 0 && !state.TrySpendFunds(choice.requiredFunds))
        {
            return;
        }

        if (choice.requiredFood > 0 && !state.TrySpendFood(choice.requiredFood))
        {
            return;
        }

        if (!state.TryConsumeMaterials(choice.materialRequirements))
        {
            return;
        }

        ApplyEffects(state, choice.effects);
    }

    private void ApplyEffects(GameState state, EventEffect[] effects)
    {
        if (effects == null)
        {
            return;
        }

        for (int i = 0; i < effects.Length; i++)
        {
            EventEffect effect = effects[i];
            if (effect == null)
            {
                continue;
            }

            if (effect.effectType == EventEffectType.AddFood)
            {
                state.AddFood(effect.amount);
            }
            else if (effect.effectType == EventEffectType.AddFunds)
            {
                state.AddFunds(effect.amount);
            }
            else if (effect.effectType == EventEffectType.AddPopulation)
            {
                state.AddPopulation(effect.amount);
            }
            else if (effect.effectType == EventEffectType.AddHappiness)
            {
                state.SetHappiness(state.Happiness + effect.amount);
            }
            else if (effect.effectType == EventEffectType.AddMaterial)
            {
                state.AddMaterial(effect.materialType, effect.materialGrade, effect.amount);
            }
        }
    }
}
