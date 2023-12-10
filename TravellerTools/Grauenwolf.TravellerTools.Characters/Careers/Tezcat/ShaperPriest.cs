﻿namespace Grauenwolf.TravellerTools.Characters.Careers.Tezcat;

abstract class ShaperPriest : NormalCareer
{
    public ShaperPriest(string assignment, CharacterBuilder characterBuilder) : base("Shaper Priest", assignment, characterBuilder)
    {
    }

    protected override int AdvancedEductionMin => 8;

    protected override bool RankCarryover => false;

    internal override void BasicTrainingSkills(Character character, Dice dice, bool all)
    {
        var roll = dice.D(6);

        if (all || roll == 1)
            character.Skills.Add("Profession", "Religion");
        if (all || roll == 2)
            character.Skills.Add("Science", "Shaper Church");
        if (all || roll == 3)
            character.Skills.Add("Admin");
        if (all || roll == 4)
            character.Skills.Add("Diplomat");
        if (all || roll == 5)
            character.Skills.Add("Persuade");
        if (all || roll == 6)
            character.Skills.Add("Electronics", "Computer");
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
                character.AddHistory("You are involved in good works in the community gaining the respect of a large segment of society. Gain an alley", dice);
                character.SocialStanding += 1;
                character.AddAlly();
                return;

            case 4:
                character.AddHistory("A notable academic consults with you about a publication or documentary they are working on. Gain a Contact in academia.", dice);
                character.AddContact();
                return;

            case 5:
                character.AddHistory("You are sent to a new community or parish to preach Shaper doctrine among the poor.", dice);
                {
                    var skillList = new SkillTemplateCollection();
                    skillList.Add("Streetwise");
                    skillList.Add("Persuade");
                    if (skillList.Count > 0)
                        character.Skills.Add(dice.Choose(skillList), 1);
                }

                return;

            case 6:
                character.AddHistory("You retreat from the mundane world for a time in the hope of a revelation, although this affects your work and relationships.", dice);
                character.SocialStanding -= dice.D(3);
                if (character.SocialStanding < 1)
                    character.SocialStanding = 1;
                character.BenefitRolls += dice.D(3);
                character.BenefitRollsPermanentDM += 1;

                return;

            case 7:
                LifeEvent(character, dice);
                return;

            case 8:
                character.AddHistory("You are chosen to represent the Shaper Church in a vid show or other highly public forum.", dice);
                {
                    var skillList = new SkillTemplateCollection();
                    skillList.Add("Carouse");
                    skillList.Add("Persuade");
                    if (skillList.Count > 0)
                        character.Skills.Add(dice.Choose(skillList), 1);
                }
                return;

            case 9:
                if (dice.NextBoolean())
                {
                    var enemies = dice.D(6);
                    character.AddHistory($"You refuse an offer of inducements to betray the Shaper Church. Gain {enemies} enemies.", dice);
                    character.AddEnemy(enemies);
                }
                else
                {
                    var cash = 10000 * dice.D(character.BenefitRolls * 2, 6);
                    character.BenefitRolls = 0;
                    character.AddHistory($"You accept an offer of inducements to betray the Shaper Church. Gain {cash.ToString("N0")} in cash.", dice);
                    character.NextTermBenefits.MusterOut = true;
                }
                return;

            case 10:
                {
                    if (dice.NextBoolean())
                    {
                        character.AddHistory($"A leader or other important personage confides in you about their highly damaging indiscretions. Gain an ally", dice);
                        character.AddAlly();
                    }
                    else
                    {
                        character.AddHistory($"You betray a leader or other important personage who confided in you about their highly damaging indiscretions. Gain an enemy", dice);
                        character.BenefitRolls += dice.D(3);
                        character.AddEnemy();
                    }
                    return;
                }
            case 11:
                character.AddHistory("Someone more charismatic but less devout than you has become your superior.", dice);
                character.LongTermBenefits.AdvancementDM = -1;
                return;

            case 12:
                var age = character.AddHistory("Your faith enjoys an explosion of popularity largely thanks to your efforts.", dice);
                Promote(character, dice, character.LastCareer!, age);
                character.LongTermBenefits.AdvancementDM = 1;
                return;
        }
    }

    internal override void Mishap(Character character, Dice dice, int age)
    {
        switch (dice.D(6))
        {
            case 1:
                character.AddHistory("Opponents of your belief system ambush you.", age);
                Injury(character, dice, true, age);
                return;

            case 2:
                character.AddHistory("You aid a follower back to the True Path but this angers a friend or relative. Gain an Enemy.", age);
                character.AddEnemy();
                return;

            case 3:
                character.AddHistory("You are caught in a scandal.", age);
                Demote(character, dice, character.LastCareer!, age);
                character.BenefitRolls -= 1;
                character.NextTermBenefits.MusterOut = false;
                return;

            case 4:
                character.AddHistory("You have been following false teachings!", age);

                var skills = new List<Skill>();
                var skillA = character.Skills["Profession", "Religion"];
                if (skillA?.Level > 0)
                    skills.Add(skillA);
                var skillB = character.Skills["Science", "Shaper Church"];
                if (skillB?.Level > 0)
                    skills.Add(skillB);

                if (skills.Count > 0)
                    dice.Choose(skills).Level -= 1;
                return;

            case 5:
                character.AddHistory("Your faith is shaken.", age);
                character.BenefitRolls -= 1;
                character.NextTermBenefits.MusterOut = false;
                return;

            case 6:
                var rivals = dice.D(3);
                character.AddHistory($"You come into conflict with a splinter group of the Shaper Church which maintains your version is the wrong one. Gain {rivals} Rivals.", age);
                character.AddRival(3);

                return;
        }
    }

    internal override bool Qualify(Character character, Dice dice)
    {
        var dm = character.IntellectDM;
        dm += -1 * character.CareerHistory.Count;

        dm += character.GetEnlistmentBonus(Career, Assignment);
        dm += QualifyDM;

        return dice.RollHigh(dm, 6);
    }

    internal override void ServiceSkill(Character character, Dice dice)
    {
        switch (dice.D(6))
        {
            case 1:
                character.Skills.Increase("Profession", "Religion");
                return;

            case 2:
                character.Skills.Increase("Science", "Shaper Church");
                return;

            case 3:
                character.Skills.Increase("Admin");
                return;

            case 4:
                character.Skills.Increase("Diplomat");
                return;

            case 5:
                character.Skills.Increase("Persuade");
                return;

            case 6:
                character.Skills.Increase("Electronics", "Computer");
                return;
        }
    }

    internal sealed override void TitleTable(Character character, CareerHistory careerHistory, Dice dice)
    {
        TitleTable(character, careerHistory, dice, true);
    }

    protected override void AdvancedEducation(Character character, Dice dice)
    {
        switch (dice.D(6))
        {
            case 1:
                character.Skills.Increase("Profession", "Religion");
                return;

            case 2:
                character.Skills.Increase(dice.Choose(SpecialtiesFor("Science")));
                return;

            case 3:
                character.Skills.Increase("Medic");
                return;

            case 4:
                character.Skills.Increase("Investigate");
                return;

            case 5:
                character.Skills.Increase("Advocate");
                return;

            case 6:
                character.Skills.Increase("Electronics", "Computer");
                return;
        }
    }

    protected void Demote(Character character, Dice dice, CareerHistory careerHistory, int? age)
    {
        string historyMessage;
        if (careerHistory.CommissionRank > 0)
        {
            careerHistory.CommissionRank -= 1;
            historyMessage = $"Demoted to {careerHistory.LongName} officer rank {careerHistory.CommissionRank}";
        }
        else
        {
            careerHistory.Rank -= 1;
            historyMessage = $"Demoted to {careerHistory.LongName} rank {careerHistory.Rank}";
        }

        var oldTitle = character.Title;
        TitleTable(character, careerHistory, dice, allowBonus: false);
        var newTitle = careerHistory.Title;
        if (oldTitle != newTitle && newTitle != null)
        {
            historyMessage += $" with the new title {newTitle}";
            character.Title = newTitle;
        }
        historyMessage += ".";
        character.AddHistory(historyMessage, age ?? character.Age);
    }

    protected override void PersonalDevelopment(Character character, Dice dice)
    {
        switch (dice.D(6))
        {
            case 1:
                character.Intellect += 1;
                return;

            case 2:
                character.Education += 1;
                return;

            case 3:
                character.SocialStanding += 1;
                return;

            case 4:
                character.Skills.Increase("Science", "Shaper Church");
                return;

            case 5:
                character.Skills.Increase("Profession", "Religion");
                return;

            case 6:
                character.Skills.Increase("Persuade");
                return;
        }
    }

    protected virtual void TitleTable(Character character, CareerHistory careerHistory, Dice dice, bool allowBonus)
    {
        switch (careerHistory.Rank)
        {
            case 0:
                careerHistory.Title = "Least Claw";
                return;

            case 1:
                careerHistory.Title = "Third Claw";
                if (allowBonus)
                    character.Skills.Increase(dice.Choose(SpecialtiesFor("Gun Combat")));
                return;

            case 2:
                careerHistory.Title = "Second Claw";
                if (allowBonus)
                    character.Skills.Increase("Leadership");
                return;

            case 3:
                careerHistory.Title = "First Claw";
                return;

            case 4:
                careerHistory.Title = "Kaltrhar";
                if (allowBonus)
                    character.Skills.Increase("Tactics", "Military");
                return;

            case 5:
                careerHistory.Title = "Shilahn";
                return;

            case 6:
                careerHistory.Title = "Shil Shintrah";
                if (allowBonus)
                    character.SocialStanding += 1;
                return;
        }
    }
}