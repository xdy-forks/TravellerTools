using Grauenwolf.TravellerTools.Names;
using System.Collections.Immutable;

namespace Grauenwolf.TravellerTools.Characters;

public class CharacterBuilderLocator
{
    readonly Dictionary<string, CharacterBuilder> m_CharacterBuilders = new(StringComparer.OrdinalIgnoreCase);
    private readonly NameGenerator m_NameGenerator;
    readonly ImmutableArray<string> m_SpeciesList;

    public CharacterBuilderLocator(string dataPath, NameGenerator nameGenerator)
    {
        void Add(CharacterBuilder builder) { m_CharacterBuilders[builder.Species] = builder; }

        Add(new HumanitiCharacterBuilder(dataPath, nameGenerator, this));
        Add(new BwapCharacterBuilder(dataPath, nameGenerator, this));

        m_SpeciesList = m_CharacterBuilders.Keys.ToImmutableArray();
        m_NameGenerator = nameGenerator;
    }

    public Character Build(CharacterBuilderOptions options)
    {
        var builder = GetCharacterBuilder(options.Species);
        return builder.Build(options);
    }

    public void BuildContacts(Dice dice, Character character)
    {
        while (character.UnusedContacts.Count > 0)
            character.Contacts.Add(CreateContact(dice, character.UnusedContacts.Dequeue(), character));
    }

    public Contact CreateContact(Dice dice, ContactType contactType, Character? character)
    {
        int PITable(int roll)
        {
            return roll switch
            {
                <= 5 => 0,
                <= 7 => 1,
                8 => 2,
                9 => 3,
                10 => 4,
                11 => 5,
                _ => 6,
            };
        }

        int AffinityTable(int roll)
        {
            return roll switch
            {
                2 => 0,
                <= 4 => 1,
                <= 6 => 2,
                <= 8 => 3,
                <= 10 => 4,
                <= 11 => 5,
                _ => 6,
            };
        }

        var user = m_NameGenerator.CreateRandomPerson(dice);

        var options = new CharacterBuilderOptions() { MaxAge = 22 + dice.D(1, 50), Gender = user.Gender, Name = $"{user.FirstName} {user.LastName}", Seed = dice.Next(), Species = GetRandomSpecies(dice) };

        var result = new Contact(contactType, options);
        RollAffinityEnmity(dice, result);
        result.Power = PITable(dice.D(2, 6));
        result.Influence = PITable(dice.D(2, 6));

        if (dice.D(2, 6) >= 8) //special!
        {
            var specialCount = 1;

            while (specialCount > 0)
            {
                specialCount -= 1;

                switch (dice.D66())
                {
                    case 11:
                        result.History.Add("Forgiveness.");
                        result.Affinity += 1;
                        break;

                    case 12:
                        result.History.Add("Relationship soured.");
                        result.Affinity -= 1;
                        result.Enmity += 1;
                        break;

                    case 13:
                        result.History.Add("Relationship altered.");
                        result.Affinity += 1;
                        result.Enmity -= 1;
                        break;

                    case 14:
                        result.History.Add("An incident occured.");
                        result.Enmity += 1;
                        break;

                    case 15:
                        switch (result.ContactType)
                        {
                            case ContactType.Enemy:
                                result.History.Add("Relationship becomes more moderate. Enemy becomes a rival.");
                                result.ContactType = ContactType.Rival;
                                break;

                            case ContactType.Ally:
                                result.History.Add("Relationship becomes more moderate. Ally becomes a contact.");
                                result.ContactType = ContactType.Contact;
                                break;

                            default:
                                result.History.Add("Relationship becomes more moderate.");
                                result.Enmity -= 1;
                                result.Affinity += 1;
                                break;
                        }
                        break;

                    case 16:
                        switch (result.ContactType)
                        {
                            case ContactType.Rival:
                                result.History.Add("Relationship becomes more moderate. Rival becomes an enemy.");
                                result.ContactType = ContactType.Enemy;
                                RollAffinityEnmity(dice, result);
                                break;

                            case ContactType.Contact:
                                result.History.Add("Relationship becomes more moderate. Contact becomes an ally.");
                                result.ContactType = ContactType.Ally;
                                RollAffinityEnmity(dice, result);
                                break;

                            case ContactType.Enemy:
                                result.History.Add("Relationship becomes more intense.");
                                result.Enmity += 1;
                                break;

                            case ContactType.Ally:
                                result.History.Add("Relationship becomes more intense.");
                                result.Affinity += 1;
                                break;
                        }
                        break;

                    case 21:
                        result.History.Add($"{result.CharacterStub.Name} gains in power.");
                        result.Power += 1;
                        break;

                    case 22:
                        result.History.Add($"{result.CharacterStub.Name} loses some of their power base.");
                        result.Power -= 1;
                        break;

                    case 23:
                        result.History.Add($"{result.CharacterStub.Name} gains in influence.");
                        result.Influence += 1;
                        break;

                    case 24:
                        result.History.Add($"{result.CharacterStub.Name}'s influence is diminished.");
                        result.Influence -= 1;
                        break;

                    case 25:
                        result.History.Add($"{result.CharacterStub.Name} gains in power and influence.");
                        result.Power += 1;
                        result.Influence += 1;
                        break;

                    case 26:
                        result.History.Add($"{result.CharacterStub.Name} is diminished in both power and influence.");
                        result.Power -= 1;
                        result.Influence -= 1; break;

                    case 31:
                        result.History.Add($"{result.CharacterStub.Name} belongs to an unusual cultural or religious group.");
                        break;

                    case 32:
                        result.History.Add($"{result.CharacterStub.Name} belongs to an uncommon alien species.");
                        break;

                    case 33:
                        result.History.Add($"{result.CharacterStub.Name} is particularly unusual, such as an artificial intelligence or very alien being. ");
                        break;

                    case 34:
                        result.History.Add($"{result.CharacterStub.Name} is actually an organisation such as a political movement or modest sized business.");
                        break;

                    case 35:
                        result.History.Add($"{result.CharacterStub.Name} is a member of an organisation which holds a generally opposite view of the Traveller.");
                        break;

                    case 36:
                        result.History.Add($"{result.CharacterStub.Name} is a questionable figure such as a criminal, pirate or disgraced noble.");
                        break;

                    case 41:
                        result.History.Add("Very bad falling out.");
                        result.Enmity = Math.Max(result.Enmity, dice.D(2, 6));
                        break;

                    case 42:
                        result.History.Add("reconciliation.");
                        result.Affinity = Math.Max(result.Affinity, dice.D(2, 6));
                        break;

                    case 43:
                        result.History.Add($"{result.CharacterStub.Name} fell on hard times.");
                        result.Power -= 1;
                        break;

                    case 44:
                        result.History.Add($"{result.CharacterStub.Name} was ruined by misfortune caused by the character.");
                        result.Power = 0;
                        result.Enmity += 1;
                        break;

                    case 45:
                        result.History.Add($"{result.CharacterStub.Name} gained influence with the character�s assistance.");
                        result.Influence += 1;
                        result.Affinity += 1;
                        break;

                    case 46:
                        result.History.Add($"{result.CharacterStub.Name} gained power at the expense of a third party who now blames the character.");
                        result.Power += 1;
                        character?.AddEnemy();
                        break;

                    case 51:
                        result.History.Add($"{result.CharacterStub.Name} is missing under suspicious circumstances.");
                        break;

                    case 52:
                        result.History.Add($"{result.CharacterStub.Name} is out of contact doing something interesting but not suspicious.");
                        break;

                    case 53:
                        result.History.Add($"{result.CharacterStub.Name} is in desperate trouble and could use the character�s help.");
                        break;

                    case 54:
                        result.History.Add($"{result.CharacterStub.Name} has had an unexpected run of good fortune lately.");
                        break;

                    case 55:
                        result.History.Add($"{result.CharacterStub.Name} is in prison or otherwise trapped somewhere.");
                        break;

                    case 56:
                        result.History.Add($"{result.CharacterStub.Name} is found or reported dead.");
                        break;

                    case 61:
                        result.History.Add($"{result.CharacterStub.Name} has life-changing event that creates new responsibilities.");
                        break;

                    case 62:
                        result.History.Add($"{result.CharacterStub.Name} has negatively life-changing event.");
                        break;

                    case 63:
                        result.History.Add($"{result.CharacterStub.Name}�s relationships have begun to affect the character.");
                        if (result.Affinity > result.Enmity)
                            character?.AddContact();
                        else if (result.Affinity < result.Enmity)
                            character?.AddRival();
                        break;

                    case 64:

                        var temp = result.Affinity;
                        result.Affinity = result.Enmity;
                        result.Enmity = temp;

                        switch (result.ContactType)
                        {
                            case ContactType.Rival:
                                result.History.Add("Relationship redefined. Rival becomes a contact.");
                                result.ContactType = ContactType.Contact;
                                break;

                            case ContactType.Contact:
                                result.History.Add("Relationship redefined. Contact becomes an rival.");
                                result.ContactType = ContactType.Rival;
                                break;

                            case ContactType.Enemy:
                                result.History.Add("Relationship redefined. Enemy becomes an ally.");
                                result.ContactType = ContactType.Ally;
                                break;

                            case ContactType.Ally:
                                result.History.Add("Relationship redefined. Ally becomes an enemy.");
                                result.ContactType = ContactType.Enemy;
                                break;
                        }
                        break;

                    case 65:
                        specialCount += 2;
                        break;

                    case 66:
                        specialCount += 3;
                        break;
                }
            }
        }

        return result;

        void RollAffinityEnmity(Dice dice, Contact result)
        {
            switch (result.ContactType)
            {
                case ContactType.Ally:
                    result.Affinity = AffinityTable(dice.D(2, 6));
                    break;

                case ContactType.Enemy:
                    result.Enmity = AffinityTable(dice.D(2, 6));
                    break;

                case ContactType.Rival:
                    result.Affinity = AffinityTable(dice.D(1, 6) - 1);
                    result.Enmity = AffinityTable(dice.D(1, 6) + 1);
                    break;

                case ContactType.Contact:
                    result.Affinity = AffinityTable(dice.D(1, 6) + 1);
                    result.Enmity = AffinityTable(dice.D(1, 6) - 1);
                    break;
            }
        }
    }

    public CharacterBuilder GetCharacterBuilder(string? species)
    {
        if (species != null && m_CharacterBuilders.TryGetValue(species, out var result))
            return result;
        else
            return m_CharacterBuilders["Humaniti"];
    }

    public string GetRandomSpecies(Dice dice)
    {
        return dice.Choose(m_SpeciesList);
    }
}