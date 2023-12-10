﻿namespace Grauenwolf.TravellerTools.Characters.Careers.Humaniti;

abstract class Scout : NormalCareer
{
    public Scout(string assignment, CharacterBuilder characterBuilder) : base("Scout", assignment, characterBuilder)
    {
    }

    protected override int AdvancedEductionMin => 8;

    protected override bool RankCarryover => false;

    internal override void BasicTrainingSkills(Character character, Dice dice, bool all)
    {
        var roll = dice.D(6);

        if (all || roll == 1)
            character.Skills.Add("Pilot");
        if (all || roll == 2)
            character.Skills.Add("Survival");
        if (all || roll == 3)
            character.Skills.Add("Mechanic");
        if (all || roll == 4)
            character.Skills.Add("Astrogation");
        if (all || roll == 5)
            character.Skills.Add("Vacc Suit");
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
                if (dice.NextBoolean())
                {
                    if (dice.RollHigh(character.Skills.BestSkillLevel("Pilot"), 8))
                    {
                        character.AddHistory("Your ship is ambushed by enemy vessels. You successfully ran to the jump point.", dice);
                        character.Skills.Add("Electronics", "Sensors", 1);
                    }
                    else
                    {
                        character.AddHistory("Your ship is ambushed by enemy vessels. Attempted to run but caught and ship is destroyed.", dice);
                        character.NextTermBenefits.MusterOut = true;
                    }
                }
                else
                {
                    if (dice.RollHigh(character.Skills.BestSkillLevel("Persuade"), 10))
                    {
                        character.AddHistory("Your ship is ambushed by enemy vessels. You successfully bargain with them.", dice);
                        character.Skills.Add("Electronics", "Sensors", 1);
                    }
                    else
                    {
                        character.AddHistory("Your ship is ambushed by enemy vessels. Attempted to bargain with them but fail and the ship is destroyed.", dice);
                        character.NextTermBenefits.MusterOut = true;
                    }
                }
                return;

            case 4:
                character.AddHistory("You survey an alien world.", dice);
                {
                    var skillList = new SkillTemplateCollection();
                    skillList.Add("Animals", "Handling");
                    skillList.Add("Animals", "Training");
                    skillList.Add("Survival");
                    skillList.Add("Recon");
                    skillList.AddRange(SpecialtiesFor("Science"));
                    skillList.RemoveOverlap(character.Skills, 1);
                    if (skillList.Count > 0)
                        character.Skills.Add(dice.Choose(skillList), 1);
                }
                return;

            case 5:
                character.AddHistory("Perform an exemplary service for the scouts.", dice);
                character.BenefitRollDMs.Add(1);
                return;

            case 6:
                character.AddHistory("Spend several years jumping from world to world in your scout ship.", dice);
                {
                    var skillList = new SkillTemplateCollection();
                    skillList.Add("Astrogation");
                    skillList.AddRange(SpecialtiesFor("Electronics"));
                    skillList.Add("Navigation");
                    skillList.Add("Pilot", "Small craft");
                    skillList.Add("Mechanic");
                    skillList.RemoveOverlap(character.Skills, 1);
                    if (skillList.Count > 0)
                        character.Skills.Add(dice.Choose(skillList), 1);
                }

                return;

            case 7:
                LifeEvent(character, dice);
                return;

            case 8:
                if (dice.RollHigh(character.Skills.BestSkillLevel("Electronics", "Deception"), 8))
                {
                    character.AddHistory("When dealing with an alien race, you have an opportunity to gather extra intelligence about them. Gain an Ally in the Imperium", dice);
                    character.AddAlly();
                    character.BenefitRollDMs.Add(2);
                }
                else
                {
                    var age = character.AddHistory("When dealing with an alien race, you botch an opportunity to gather extra intelligence about them.", dice);
                    Mishap(character, dice, age);
                    character.NextTermBenefits.MusterOut = false;
                }
                return;

            case 9:
                if (dice.RollHigh(character.Skills.BestSkillLevel("Medic", "Engineer"), 8))
                {
                    character.AddHistory("Your scout ship is one of the first on the scene to rescue the survivors of a disaster. Gain a Contact.", dice);
                    character.AddContact();
                    character.BenefitRollDMs.Add(2);
                }
                else
                {
                    var age = character.AddHistory("Your scout ship is one of the first on the scene to rescue the survivors of a disaster but you fail to help. Gain an Enemy.", dice);
                    character.AddEnemy();
                    Mishap(character, dice, age);
                    character.NextTermBenefits.MusterOut = false;
                }
                return;

            case 10:
                {
                    var age = character.AddHistory("You spend a great deal of time on the fringes of Charted Space.", dice);
                    if (dice.RollHigh(character.Skills.BestSkillLevel("Survival", "Pilot"), 8))
                    {
                        character.AddHistory("Gain a contact in an alien race.", age);
                        character.AddContact();
                        dice.Choose(character.Skills).Level += 1;
                    }
                    else
                    {
                        Mishap(character, dice, age);
                        character.NextTermBenefits.MusterOut = false;
                    }
                    return;
                }
            case 11:
                character.AddHistory("Serve as the courier for an important message from the Imperium.", dice);
                switch (dice.D(2))
                {
                    case 1:
                        character.Skills.Increase("Diplomat");
                        return;

                    case 2:
                        character.CurrentTermBenefits.AdvancementDM += 4;
                        return;
                }
                return;

            case 12:
                character.AddHistory("You discover a world, item or information of worth to the Imperium.", dice);
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
                character.AddHistory("Psychologically damaged by your time in the scouts.", age);
                if (dice.NextBoolean())
                    character.Intellect += -1;
                else
                    character.SocialStanding += -1;
                return;

            case 3:
                character.AddHistory("Your ship is damaged, and you have to hitch-hike your way back across the stars to the nearest scout base.", age);
                int countC = dice.D(6);
                int countE = dice.D(3);
                character.AddHistory($"Gain {countC} Contacts. Gain {countE} Enemies.", age);
                character.AddContact(countC);
                character.AddEnemy(countE);
                return;

            case 4:
                character.AddHistory("You inadvertently cause a conflict between the Imperium and a minor world or race. Gain a Rival.", age);
                character.AddRival();
                character.Skills.Add("Diplomat", 1);
                return;

            case 5:
                character.AddHistory("You have no idea what happened to you – they found your ship drifting on the fringes of friendly space.", age);
                return;

            case 6:
                Injury(character, dice, age);
                return;
        }
    }

    internal override bool Qualify(Character character, Dice dice)
    {
        var dm = character.IntellectDM;
        dm += -1 * character.CareerHistory.Count;

        dm += character.GetEnlistmentBonus(Career, Assignment);
        dm += QualifyDM;

        return dice.RollHigh(dm, 5);
    }

    internal override void ServiceSkill(Character character, Dice dice)
    {
        switch (dice.D(6))
        {
            case 1:
                if (dice.NextBoolean())
                    character.Skills.Increase("Pilot", "Small craft");
                else
                    character.Skills.Increase("Pilot", "Spacecraft");
                return;

            case 2:
                character.Skills.Increase("Survival");
                return;

            case 3:
                character.Skills.Increase("Mechanic");
                return;

            case 4:
                character.Skills.Increase("Astrogation");
                return;

            case 5:
                character.Skills.Increase("Vacc Suit");
                return;

            case 6:
                character.Skills.Increase(dice.Choose(SpecialtiesFor("Gun Combat")));
                return;
        }
    }

    internal override void TitleTable(Character character, CareerHistory careerHistory, Dice dice)
    {
        switch (careerHistory.Rank)
        {
            case 0:
                return;

            case 1:
                careerHistory.Title = "Scout";
                character.Skills.Add("Vacc Suit", 1);
                return;

            case 2:
                return;

            case 3:
                careerHistory.Title = "Senior Scout";
                var skillList = new SkillTemplateCollection(SpecialtiesFor("Pilot"));
                skillList.RemoveOverlap(character.Skills, 1);
                if (skillList.Count > 0)
                    character.Skills.Add(dice.Choose(skillList), 1);
                return;

            case 4:
                return;

            case 5:
                return;

            case 6:
                return;
        }
    }

    protected override void AdvancedEducation(Character character, Dice dice)
    {
        switch (dice.D(6))
        {
            case 1:
                character.Skills.Increase("Medic");
                return;

            case 2:
                character.Skills.Increase("Navigation");
                return;

            case 3:
                character.Skills.Increase(dice.Choose(SpecialtiesFor("Seafarer")));
                return;

            case 4:
                character.Skills.Increase("Explosives");
                return;

            case 5:
                character.Skills.Increase(dice.Choose(SpecialtiesFor("Science")));
                return;

            case 6:
                character.Skills.Increase("Jack-of-all-Trades");
                return;
        }
    }

    protected override void PersonalDevelopment(Character character, Dice dice)
    {
        switch (dice.D(6))
        {
            case 1:
                character.Strength += 1;
                return;

            case 2:
                character.Dexterity += 1;
                return;

            case 3:
                character.Endurance += 1;
                return;

            case 4:
                character.Intellect += 1;
                return;

            case 5:
                character.Education += 1;
                return;

            case 6:
                character.Skills.Increase("Jack-of-all-Trades");
                return;
        }
    }
}