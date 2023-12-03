﻿using System.Collections.Immutable;

namespace Grauenwolf.TravellerTools.Characters.Careers;

abstract class Agent : NormalCareer
{
    private ImmutableArray<NormalCareer> m_Careers;

    public Agent(string assignment, Book book) : base("Agent", assignment, book)
    {
        var careers = new List<NormalCareer>
        {
            new Corporate(book),
            new Worker(book),
            new Colonist(book),
            new Thief(book),
            new Enforcer(book),
            new Pirate(book)
        };
        m_Careers = careers.ToImmutableArray();
    }

    protected override int AdvancedEductionMin => 8;

    protected override bool RankCarryover => false;

    internal override void BasicTrainingSkills(Character character, Dice dice, bool all)
    {
        var roll = dice.D(6);

        if (all || roll == 1)
            character.Skills.Add("Streetwise");
        if (all || roll == 2)
            character.Skills.Add("Drive");
        if (all || roll == 3)
            character.Skills.Add("Investigate");
        if (all || roll == 4)
            character.Skills.Add("Flyer");
        if (all || roll == 5)
            character.Skills.Add("Recon");
        if (all || roll == 6)
            character.Skills.Add("Gun Combat");
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
                    var age = character.AddHistory("An investigation takes on a dangerous turn.", dice);

                    if (dice.RollHigh(character.Skills.BestSkillLevel("Investigate", "Streetwise"), 8))
                    {
                        var skillList = new SkillTemplateCollection();
                        skillList.Add("Deception");
                        skillList.Add("Jack-of-All-Trades");
                        skillList.Add("Persuade");
                        skillList.Add("Tactics");
                        character.Skills.Increase(dice.Choose(skillList));
                    }
                    else
                    {
                        Mishap(character, dice, age);
                    }
                    return;
                }
            case 4:
                character.AddHistory("Rewarded for a successful mission.", dice);
                character.BenefitRollDMs.Add(1);
                return;

            case 5:
                var count = dice.D(3);
                character.AddHistory($"Established a network of contacts. Gain {count} contacts.", dice);
                character.AddContact(count);
                return;

            case 6:
                character.AddHistory("Advanced training in a specialist field.", dice);
                if (dice.RollHigh(character.EducationDM, 8))
                {
                    dice.Choose(character.Skills).Level += 1;
                }
                return;

            case 7:
                LifeEvent(character, dice);
                return;

            case 8:
                {
                    var age = character.AddHistory("Go undercover to investigate an enemy.", dice);
                    character.AddEnemy();

                    var career = dice.Choose(m_Careers);

                    if (dice.RollHigh(character.Skills.GetLevel("Deception"), 8))
                    {
                        career.Event(character, dice);
                        career.AssignmentSkills(character, dice);
                    }
                    else
                    {
                        career.Mishap(character, dice, age);
                    }
                }
                return;

            case 9:
                character.AddHistory("You go above and beyond the call of duty.", dice);
                character.CurrentTermBenefits.AdvancementDM += 2;
                return;

            case 10:
                character.AddHistory("Given specialist training in vehicles.", dice);
                {
                    var skillList = new SkillTemplateCollection();
                    skillList.AddRange(SpecialtiesFor("Drive"));
                    skillList.AddRange(SpecialtiesFor("Flyer"));
                    skillList.AddRange(SpecialtiesFor("Pilot"));
                    skillList.AddRange(SpecialtiesFor("Gunner"));
                    skillList.RemoveOverlap(character.Skills, 1);
                    if (skillList.Count > 0)
                        character.Skills.Add(dice.Choose(skillList), 1);
                }
                return;

            case 11:
                character.AddHistory("Befriended by a senior agent.", dice);
                switch (dice.D(2))
                {
                    case 1:
                        character.Skills.Increase("Investigate");
                        return;

                    case 2:
                        character.CurrentTermBenefits.AdvancementDM += 4;
                        return;
                }
                return;

            case 12:
                character.AddHistory("Uncover a major conspiracy against your employers.", dice);
                character.CurrentTermBenefits.AdvancementDM += 100;
                return;
        }
    }

    internal override decimal MedicalPaymentPercentage(Character character, Dice dice)
    {
        var roll = dice.D(2, 6) + (character.LastCareer?.Rank ?? 0);
        if (roll >= 12)
            return 1.0M;
        if (roll >= 8)
            return 0.75M;
        if (roll >= 4)
            return 0.50M;
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
                character.AddHistory("Life ruined by a criminal gang. Gain the gang as an Enemy", age);
                character.AddEnemy();
                return;

            case 3:
                character.AddHistory("Hard times caused by a lack of interstellar trade costs you your job.", age);
                character.SocialStanding += -1;
                return;

            case 4:
                if (dice.NextBoolean())
                {
                    character.AddHistory("Accepted a deal with criminal or other figure under investigation.", age);
                }
                else
                {
                    character.AddHistory("Refused a deal with criminal or other figure under investigation. Gain an Enemy", age);
                    character.Skills.Increase(dice.Choose(RandomSkills));
                    character.AddEnemy();
                }
                return;

            case 5:
                character.AddHistory("Your work ends up coming home with you, and someone gets hurt. Contact or ally takes the worst of 2 injury rolls.", age);
                return;

            case 6:
                Injury(character, dice, false, age);
                return;
        }
    }

    internal override bool Qualify(Character character, Dice dice)
    {
        var dm = character.IntellectDM;
        dm += -1 * character.CareerHistory.Count;

        dm += character.GetEnlistmentBonus(Career, Assignment);

        return dice.RollHigh(dm, 6);
    }

    internal override void ServiceSkill(Character character, Dice dice)
    {
        switch (dice.D(6))
        {
            case 1:
                character.Skills.Increase("Streetwise");
                return;

            case 2:
                character.Skills.Increase(dice.Choose(SpecialtiesFor("Drive")));
                return;

            case 3:
                character.Skills.Increase("Investigate");
                return;

            case 4:
                character.Skills.Increase(dice.Choose(SpecialtiesFor("Flyer")));
                return;

            case 5:
                character.Skills.Increase("Recon");
                return;

            case 6:
                character.Skills.Increase(dice.Choose(SpecialtiesFor("Gun Combat")));
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
                character.Skills.Increase(dice.Choose(SpecialtiesFor("Language")));
                return;

            case 3:
                character.Skills.Increase("Explosives");
                return;

            case 4:
                character.Skills.Increase("Medic");
                return;

            case 5:
                character.Skills.Increase("Vacc Suit");
                return;

            case 6:
                character.Skills.Increase(dice.Choose(SpecialtiesFor("Electronics")));
                return;
        }
    }

    protected override void PersonalDevelopment(Character character, Dice dice)
    {
        switch (dice.D(6))
        {
            case 1:
                character.Skills.Increase(dice.Choose(SpecialtiesFor("Gun Combat")));
                return;

            case 2:
                character.Dexterity += 1;
                return;

            case 3:
                character.Endurance += 1;
                return;

            case 4:
                character.Skills.Increase(dice.Choose(SpecialtiesFor("Melee")));
                return;

            case 5:
                character.Intellect += 1;
                return;

            case 6:
                character.Skills.Increase(dice.Choose(SpecialtiesFor("Athletics")));
                return;
        }
    }
}
