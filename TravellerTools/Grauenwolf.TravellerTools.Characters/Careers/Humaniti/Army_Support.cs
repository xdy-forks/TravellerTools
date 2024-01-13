﻿namespace Grauenwolf.TravellerTools.Characters.Careers.Humaniti;

class Army_Support(CharacterBuilder characterBuilder) : Army("Army Support", characterBuilder)
{
    protected override string AdvancementAttribute => "Edu";

    protected override int AdvancementTarget => 7;

    protected override string SurvivalAttribute => "End";

    protected override int SurvivalTarget => 5;

    internal override void AssignmentSkills(Character character, Dice dice)
    {
        switch (dice.D(6))
        {
            case 1:
                character.Skills.Increase("Mechanic");

                return;

            case 2:
                {
                    var skillList = new SkillTemplateCollection();
                    skillList.AddRange(SpecialtiesFor(character, "Drive"));
                    skillList.AddRange(SpecialtiesFor(character, "Flyer"));
                    character.Skills.Increase(dice.Choose(skillList));
                }
                return;

            case 3:
                character.Skills.Increase(dice.Choose(SpecialtiesFor(character, "Profession")));
                return;

            case 4:
                character.Skills.Increase("Explosives");
                return;

            case 5:
                character.Skills.Increase("Electronics", "Comms");
                return;

            case 6:
                character.Skills.Increase("Medic");
                return;
        }
    }
}
