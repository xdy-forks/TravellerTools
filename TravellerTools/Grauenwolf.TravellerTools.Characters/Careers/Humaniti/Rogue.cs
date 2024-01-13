﻿namespace Grauenwolf.TravellerTools.Characters.Careers.Humaniti;

abstract class Rogue(string assignment, CharacterBuilder characterBuilder) : NormalCareer("Rogue", assignment, characterBuilder)
{
    protected override int AdvancedEductionMin => 10;

    protected override bool RankCarryover => false;

    internal override void BasicTrainingSkills(Character character, Dice dice, bool all)
    {
        var roll = dice.D(6);

        if (all || roll == 1)
            character.Skills.Add("Deception");
        if (all || roll == 2)
            character.Skills.Add("Recon");
        if (all || roll == 3)
            character.Skills.Add("Athletics");
        if (all || roll == 4)
            character.Skills.Add("Gun Combat");
        if (all || roll == 5)
            character.Skills.Add("Stealth");
        if (all || roll == 6)
            character.Skills.Add("Streetwise");
    }

    internal override void Event(Character character, Dice dice)
    {
        switch (dice.D(2, 6))
        {
            case 2:
                MishapRollAge(character, dice);
                character.NextTermBenefits.MusterOut = false;
                return;

            case 3:
                {
                    var age = character.AddHistory($"Arrested and charged.", dice);
                    switch (dice.D(2))
                    {
                        case 1:
                            if (dice.RollHigh(character.Skills.GetLevel("Advocate"), 8))
                            {
                                character.AddHistory($"Successfully defended self.", age);
                            }
                            else
                            {
                                character.AddHistory($"Failed to defend self. Gain an Enemy and go to prison.", age);
                                character.AddEnemy();
                                character.NextTermBenefits.MustEnroll = "Prisoner";
                            }
                            return;

                        case 2:
                            character.AddHistory($"Hired a lawyer to beat the charges.", age);
                            character.BenefitRolls += -1;
                            return;
                    }
                }
                return;

            case 4:
                character.AddHistory($"Involved in the planning of an impressive heist.", dice);
                {
                    var skillList = new SkillTemplateCollection();
                    skillList.AddRange(SpecialtiesFor(character, "Electronics"));
                    skillList.Add("Mechanic");
                    skillList.RemoveOverlap(character.Skills, 1);
                    if (skillList.Count > 0)
                        character.Skills.Add(dice.Choose(skillList), 1);
                }

                return;

            case 5:
                character.AddHistory($"Crime pays off. Gain victim as Enemy.", dice);
                character.AddEnemy();
                character.BenefitRollDMs.Add(2);
                return;

            case 6:
                switch (dice.D(2))
                {
                    case 1:
                        character.AddHistory($"Backstab a fellow rogue for personal gain.", dice);
                        character.CurrentTermBenefits.AdvancementDM += 4;
                        return;

                    case 2:
                        character.AddHistory($"Refuse to backstab a fellow rogue for personal gain. Gain an Ally", dice);
                        character.AddAlly();
                        return;
                }
                return;

            case 7:
                LifeEvent(character, dice);
                return;

            case 8:
                character.AddHistory($"{character.Name} spend months in the dangerous criminal underworld.", dice);
                {
                    var skillList = new SkillTemplateCollection();
                    skillList.Add("Streetwise");
                    skillList.Add("Stealth");
                    skillList.AddRange(SpecialtiesFor(character, "Melee"));
                    skillList.AddRange(SpecialtiesFor(character, "Gun Combat"));
                    skillList.RemoveOverlap(character.Skills, 1);
                    if (skillList.Count > 0)
                        character.Skills.Add(dice.Choose(skillList), 1);
                }
                return;

            case 9:
                {
                    var age = character.AddHistory($"Involved in a feud with a rival criminal organization.", dice);
                    if (dice.RollHigh(character.Skills.BestSkillLevel("Stealth", "Gun Combat"), 8))
                        character.BenefitRolls += 1;
                    else
                        Injury(character, dice, age);
                    return;
                }

            case 10:
                character.AddHistory($"Involved in a gambling ring.", dice);
                character.Skills.Add("Gambler", 1);
                if (character.BenefitRolls > 0)
                {
                    if (dice.RollHigh(character.Skills.GetLevel("Gambler"), 8))
                        character.BenefitRolls = (int)Math.Ceiling(1.5 * character.BenefitRolls);
                    else
                        character.BenefitRolls = 0;
                }
                return;

            case 11:
                character.AddHistory($"A crime lord considers {character.Name} his protégé.", dice);
                switch (dice.D(2))
                {
                    case 1:
                        character.Skills.Add("Tactics", "Military", 1);
                        return;

                    case 2:
                        character.CurrentTermBenefits.AdvancementDM += 4;
                        return;
                }
                return;

            case 12:
                character.AddHistory($"{character.Name} commit a legendary crime.", dice);
                character.CurrentTermBenefits.AdvancementDM += 100;
                return;
        }
    }

    internal override decimal MedicalPaymentPercentage(Character character, Dice dice)
    {
        var roll = dice.D(2, 6) + (character.LastCareer?.Rank ?? 0);
        if (roll >= 12)
            return 0.75M;
        if (roll >= 8)
            return 0.50M;
        if (roll >= 4)
            return 0.00M;
        return 0;
    }

    internal override void Mishap(Character character, Dice dice, int age)
    {
        switch (dice.D(6))
        {
            case 1:
                Injury(character, dice, true, age);
                return;

            case 2:
                character.AddHistory($"Arrested", age);
                character.NextTermBenefits.MustEnroll = "Prisoner";
                return;

            case 3:
                character.AddHistory($"Betrayed by a friend. One of {character.Name}'s Contacts or Allies betrays {character.Name}, ending {character.Name}'s career. That Contact or Ally becomes a Rival or Enemy.", age);
                if (dice.D(2, 6) == 2)
                {
                    character.NextTermBenefits.MustEnroll = "Prisoner";
                }
                return;

            case 4:
                character.AddHistory($"A job goes wrong, forcing {character.Name} to flee off-planet.", age);
                {
                    var skillList = new SkillTemplateCollection();
                    skillList.Add("Deception");
                    skillList.Add("Pilot", "Small Craft");
                    skillList.Add("Pilot", "Spacecraft");
                    skillList.Add("Athletics", "Dexterity");
                    skillList.AddRange(SpecialtiesFor(character, "Gunner"));
                    skillList.RemoveOverlap(character.Skills, 1);
                    if (skillList.Count > 0)
                        character.Skills.Add(dice.Choose(skillList), 1);
                }

                return;

            case 5:
                character.AddHistory($"A police detective or rival criminal forces {character.Name} to flee and vows to hunt {character.Name} down. Gain an Enemy.", age);
                character.AddEnemy();
                return;

            case 6:
                Injury(character, dice, false, age);
                return;
        }
    }

    internal override bool Qualify(Character character, Dice dice, bool isPrecheck)
    {
        var dm = character.DexterityDM;
        dm += -1 * character.CareerHistory.Count;

        dm += character.GetEnlistmentBonus(Career, Assignment);
        dm += QualifyDM;

        return dice.RollHigh(dm, 6, isPrecheck);
    }

    internal override void ServiceSkill(Character character, Dice dice)
    {
        switch (dice.D(6))
        {
            case 1:
                character.Skills.Increase("Deception");
                return;

            case 2:
                character.Skills.Increase("Recon");
                return;

            case 3:
                character.Skills.Increase(dice.Choose(SpecialtiesFor(character, "Athletics")));
                return;

            case 4:
                character.Skills.Increase(dice.Choose(SpecialtiesFor(character, "Gun Combat")));
                return;

            case 5:
                character.Skills.Increase("Stealth");
                return;

            case 6:
                character.Skills.Increase("Streetwise");
                return;
        }
    }

    protected override void AdvancedEducation(Character character, Dice dice)
    {
        switch (dice.D(6))
        {
            case 1:
                character.Skills.Increase(dice.Choose(SpecialtiesFor(character, "Electronics")));
                return;

            case 2:
                character.Skills.Increase("Navigation");
                return;

            case 3:
                character.Skills.Increase("Medic");
                return;

            case 4:
                character.Skills.Increase("Investigate");
                return;

            case 5:
                character.Skills.Increase("Broker");
                return;

            case 6:
                character.Skills.Increase("Advocate");
                return;
        }
    }

    protected override void PersonalDevelopment(Character character, Dice dice)
    {
        switch (dice.D(6))
        {
            case 1:
                character.Skills.Increase("Carouse");
                return;

            case 2:
                character.Dexterity += 1;
                return;

            case 3:
                character.Endurance += 1;
                return;

            case 4:
                character.Skills.Increase("Gambler");
                return;

            case 5:
                character.Skills.Increase(dice.Choose(SpecialtiesFor(character, "Melee")));
                return;

            case 6:
                character.Skills.Increase(dice.Choose(SpecialtiesFor(character, "Gun Combat")));
                return;
        }
    }
}
