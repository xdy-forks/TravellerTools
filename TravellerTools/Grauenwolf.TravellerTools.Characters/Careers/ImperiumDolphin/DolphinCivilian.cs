﻿namespace Grauenwolf.TravellerTools.Characters.Careers.ImperiumDolphin;

abstract class DolphinCivilian(string assignment, CharacterBuilder characterBuilder) : NormalCareer("Dolphin Civilian", assignment, characterBuilder)
{
    protected override int AdvancedEductionMin => 8;
    protected override bool RankCarryover => false;

    internal override void BasicTrainingSkills(Character character, Dice dice, bool all)
    {
        var roll = dice.D(6);

        if (all || roll == 1)
            character.Skills.Add("Athletics");
        if (all || roll == 2)
            character.Skills.Add("Athletics");
        if (all || roll == 3)
            character.Skills.Add("Melee", "Natural");
        if (all || roll == 4)
            character.Skills.Add("Melee", "Natural");
        if (all || roll == 5)
            character.Skills.Add("Carouse");
        if (all || roll == 6)
            character.Skills.Add("Recon");
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
                Skill? skill;

                if (dice.NextBoolean())
                {
                    var skills = new[] { "Advocate", "Diplomat" };
                    skill = character.Skills.BestSkill(skills) ?? new Skill("Diplomat");
                    var dm = character.Skills.BestSkillLevel(skills);

                    if (dice.RollHigh(dm, 8))
                    {
                        character.AddHistory("Rivalries between another race and Dolphins over your world’s ocean resources lead to conflict. Through diplomacy your faction was vindicated.", dice);
                        character.CurrentTermBenefits.AdvancementDM += 2;
                    }
                    else
                    {
                        character.AddHistory("Rivalries between another race and Dolphins over your world’s ocean resources lead to conflict. You try diplomacy but it fails.", dice);
                        character.BenefitRolls -= 1;
                    }
                }
                else
                {
                    var skills = new[] { "Explosives", "Gun Combat", "Tactics" };
                    skill = character.Skills.BestSkill(skills) ?? new Skill("Diplomat");
                    var dm = character.Skills.BestSkillLevel(skills);
                    if (dice.RollHigh(dm, 8))
                    {
                        character.AddHistory("Rivalries between another race and Dolphins over your world’s ocean resources lead to conflict. You succeed through violence.", dice);
                    }
                    else
                    {
                        var age = character.AddHistory("Rivalries between another race and Dolphins over your world’s ocean resources lead to conflict. You try violence and are injured in the process.", dice);
                        Injury(character, dice, age);
                        character.CurrentTermBenefits.AdvancementDM += -2;
                    }
                }

                if (dice.NextBoolean())
                    character.Skills.Increase("Leadership");
                else
                    character.Skills.Increase(skill.Name, skill.Specialty);

                return;

            case 4:
                character.AddHistory("You volunteer for a research project to test new waldo equipment or cybernetics to improve the lives of \r\nother Dolphins. Gain a Contact in the research field.", dice);
                if (dice.RollHigh(character.EducationDM, 7))
                    character.Skills.Add("Science", "Cybernetics");
                return;

            case 5:
                character.AddHistory("You have a chance to travel offworld on business and spend a long time in space.", dice);
                {
                    var skills = new SkillTemplateCollection();
                    skills.AddRange(SpecialtiesFor("Pilot"));
                    skills.RemoveOverlap(character.Skills, 1); //Only level 1 is allowed for this skill.
                    skills.Add("Athletics", "Dexterity");
                    skills.Add("Electronics", "Comms");
                    skills.Add("Electronics", "Sensors");

                    character.Skills.Increase(dice.Choose(skills));
                }

                return;

            case 6:
                if (dice.RollHigh(character.StrengthDM, 7))
                {
                    character.AddHistory("You rescue a high-status swimmer who was lost at sea. Gain an Ally with SOC 10+", dice);
                    character.AddAlly();
                }
                else
                {
                    character.AddHistory("You fail to rescue a high-status swimmer who was lost at sea.", dice);
                }
                return;

            case 7:
                LifeEvent(character, dice);
                return;

            case 8:
                character.AddHistory("Spend an extensive period living out of water among humans.", dice);
                {
                    var skills = new SkillTemplateCollection();
                    skills.Add("Vacc Suit");
                    skills.Add("Diplomat");
                    character.Skills.Increase(dice.Choose(skills));
                }
                return;

            case 9:
                if (dice.NextBoolean())
                {
                    character.AddHistory("Your companions are attacked by fierce sea creatures! You flee gain one of the survivors as an Enemy.", dice);
                    character.AddEnemy();
                }
                else
                {
                    var dm = character.Skills.BestSkillLevel("Gun Combat", "Natural");
                    if (dice.RollHigh(dm, 7))
                    {
                        character.AddHistory("Your companions are attacked by fierce sea creatures! You stay and fight, rescuing them. Gain an ally.", dice);
                        character.AddAlly();
                    }
                    else
                    {
                        var age = character.AddHistory("Your companions are attacked by fierce sea creatures! You stay and fight, getting injured in the process.", dice);
                        Injury(character, dice, age);
                    }
                }
                return;

            case 10:
                if (dice.RollHigh(Math.Max(character.EnduranceDM, character.Skills.BestSkillLevel("Survival")), 8))
                {
                    character.AddHistory("You succeed at a dangerous mission to rescue people in a sinking vessel or underwater city.", dice);
                }
                else
                {
                    character.AddHistory("You fail a dangerous mission to rescue people in a sinking vessel or underwater city.", dice);
                }
                dice.Choose(character.Skills).Level += 1;
                return;

            case 11:
                character.AddHistory("You discover a sunken vessel that contains valuable salvage. ", dice);
                character.BenefitRolls += 1;
                return;

            case 12:
                character.AddHistory("You rise to a position of responsibility.", dice);
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
                character.AddHistory("One of your friends becomes involved with a revolutionary political faction advocating Dolphin \r\nrights. You are caught up in the investigation and lose your job.", age);
                return;

            case 3:
                if (dice.RollHigh(character.Skills.BestSkillLevel("Melee", "Gun Combat"), 8))
                {
                    character.AddHistory("A war or insurgency erupts on your world. You are drafted into the military.", age);
                    character.NextTermBenefits.MustEnroll = "Dolphin Military";
                }
                else
                    character.AddHistory("To war or insurgency erupts on your world. Your business or home is destroyed, forcing you into exile.", age);

                return;

            case 4:
                character.AddHistory("A purist political party comes to power on your world making trouble for uplifted Dolphins and costing you your job. Gain a purist political militant as an Enemy", age);
                character.SocialStanding -= 1;
                return;

            case 5:
                character.AddHistory("An accident leaves you caught on land without a travel suit. You suffer serious dehydration and \r\nrequire lengthy hospitalisation.", age);
                character.Endurance += -2;
                return;

            case 6:

                var dm = Math.Max(character.SocialStandingDM, character.Skills.BestSkillLevel("Advocate"));
                if (dice.RollHigh(dm, 8))
                {
                    character.AddHistory("You are accidently trapped or shot by fishermen. You successfully sue.", age);
                    character.BenefitRolls += 2;
                }
                else
                    character.AddHistory("You are accidently trapped or shot by fishermen.", age);
                return;
        }
    }

    internal override void ServiceSkill(Character character, Dice dice)
    {
        switch (dice.D(6))
        {
            case 1:
                character.Skills.Increase(dice.Choose(SpecialtiesFor("Athletics")));
                return;

            case 2:
                character.Skills.Increase(dice.Choose(SpecialtiesFor("Athletics")));
                return;

            case 3:
                character.Skills.Increase("Melee", "Natural");
                return;

            case 4:
                character.Skills.Increase("Melee", "Natural");
                return;

            case 5:
                character.Skills.Increase("Carouse");
                return;

            case 6:
                character.Skills.Increase("Recon");
                return;
        }
    }

    protected override void AdvancedEducation(Character character, Dice dice)
    {
        switch (dice.D(6))
        {
            case 1:
                character.Skills.Increase("Advocate");
                return;

            case 2:
                character.Skills.Increase("Art");
                return;

            case 3:
                character.Skills.Increase("Vacc Suit");
                return;

            case 4:
                character.Skills.Increase("Leadership");
                return;

            case 5:
                character.Skills.Increase("Navigation");
                return;

            case 6:
                character.Skills.Increase("Science", "History");
                return;
        }
    }

    protected override void PersonalDevelopment(Character character, Dice dice)
    {
        switch (dice.D(6))
        {
            case 1:
                character.Dexterity += 1;
                return;

            case 2:
                character.Endurance += 1;
                return;

            case 3:
                character.Intellect += 1;
                return;

            case 4:
                character.Strength += 1;
                return;

            case 5:
                character.Education += 2;
                return;

            case 6:
                character.Skills.Increase("Survival");
                return;
        }
    }
}
