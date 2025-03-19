namespace DataGenerator;

public static class Inspiration
{
    public static IEnumerable<string> Genres { get; } =
    [
        "Cyberpunk",
        "Cozy Mystery",
        "Solarpunk",
        "Grimdark",
        "Magical Realism",
        "Body Horror",
        "Space Opera",
        "Folk Horror",
        "Slice-of-Life",
        "New Weird",
    ];

    public static IEnumerable<string> Settings { get; } =
    [
        "Abandoned Space Station",
        "Sentient Forest",
        "Floating City",
        "Underground Bunker",
        "Haunted Marketplace",
        "Infinite Library",
        "Melting Glacier",
        "Neon Noir Metropolis",
        "Deserted Carnival",
        "Quantum Realm",
    ];

    public static IEnumerable<string> Themes { get; } =
    [
        "Memory Theft",
        "Collective Amnesia",
        "Artificial Afterlife",
        "Genetic Identity Crisis",
        "Time Debt",
        "Digital Immortality",
        "Forbidden Knowledge",
        "Survival Guilt",
        "Ancestral Curse",
        "Reverse Aging",
    ];

    public static IEnumerable<string> ConflictTypes { get; } =
    [
        "Man vs. Algorithm",
        "Betrayal by a Clone",
        "Resource Scarcity",
        "Cultural Assimilation",
        "Sentient Disease",
        "Ecological Collapse",
        "Stolen Legacy",
        "Cursed Bargain",
        "A.I. Uprising",
        "Family vs. Duty",
    ];

    public static IEnumerable<string> CharacterArchetypes { get; } =
    [
        "Amnesiac God",
        "Reluctant Prophet",
        "Rogue Historian",
        "Ghost Detective",
        "Immortal Child",
        "Sentient AI Prisoner",
        "Fallen Celebrity",
        "Memory Thief",
        "Time Refugee",
        "Shapeshifting Spy",
    ];

    public static IEnumerable<string> MacGuffins { get; } =
    [
        "Self-Writing Book",
        "Cursed Mirror",
        "Sentient Weapon",
        "DNA Archive",
        "Black Hole Seed",
        "Forgiveness Algorithm",
        "Time-Lock Key",
        "Dream Vial",
        "Memory Coin",
        "Lost Diary",
    ];

    public static IEnumerable<string> TimeAndScale { get; } =
    [
        "Time Loop",
        "Generation Ship",
        "Microverse",
        "Parallel Timelines",
        "Eternal Winter",
        "Post-Apocalyptic Revival",
        "Prehistoric Future",
        "Quantum Leap",
        "Accelerated Evolution",
        "Frozen Civilization",
    ];

    public static IEnumerable<string> SocialStructures { get; } =
    [
        "Meritocracy Collapse",
        "Caste System",
        "Hivemind Democracy",
        "Barter-Only Economy",
        "Memory-Based Currency",
        "Underground Rebellion",
        "Algorithmic Government",
        "Ancestor Worship Cult",
        "Non-Hierarchical Colony",
        "Sentient City Council",
    ];

    public static IEnumerable<string> PhilosophicalQuestions { get; } =
    [
        "What defines humanity?",
        "Can lies save the world?",
        "Is forgetting a mercy?",
        "Is free will an illusion?",
        "Can machines grieve?",
        "Does history repeat?",
        "What is the cost of utopia?",
        "Can silence be a weapon?",
        "Is sacrifice ever selfish?",
        "Who owns the past?",
    ];

    public static IEnumerable<string> NatureAndEnvironment { get; } =
    [
        "Bioluminescent Ocean",
        "Sentient Weather",
        "Fungal Network",
        "Talking Mountains",
        "Acid Rain Forests",
        "Gravity Vortexes",
        "Living Architecture",
        "Artificial Oceans",
        "Migratory Islands",
        "Petrified Wildlife",
    ];

    public static IEnumerable<string> TechnologyAndMagic { get; } =
    [
        "Holographic Ghosts",
        "Emotion Harvesting",
        "Pain-Based Magic",
        "DNA Hacking",
        "Quantum Necromancy",
        "Gravity Manipulation",
        "Memory Uploads",
        "Dream Engineering",
        "Cursed Technology",
        "Sentient Fog",
    ];

    public static IEnumerable<string> EmotionalHooks { get; } =
    [
        "Fear of Oblivion",
        "Hope as Rebellion",
        "Love as Sacrifice",
        "Rage as Fuel",
        "Loneliness in Crowds",
        "Guilt-Driven Redemption",
        "Trust Rebuilt",
        "Curiosity Kills",
        "Joy as Resistance",
        "Grief as Time Travel",
    ];

    public static IEnumerable<string> Prompts { get; } =
    [
        "Define the Core Question or Spark",
        "Identify the Central Conflict",
        "Assign a Setting (Flexible)",
        "Create a Unique Character",
        "Introduce a Compelling MacGuffin",
        "Craft a Twist Ending",
        "Weave in a Subtle Theme",
        "Add a Dash of Humor",
        "Incorporate a Moral Dilemma",
        "Introduce a Secondary Antagonist",
    ];

    public static Dictionary<string, IEnumerable<string>> Concepts { get; } = new()
    {
        [nameof(Genres)] = Genres,
        [nameof(Settings)] = Settings,
        [nameof(Themes)] = Themes,
        [nameof(ConflictTypes)] = ConflictTypes,
        [nameof(CharacterArchetypes)] = CharacterArchetypes,
        [nameof(MacGuffins)] = MacGuffins,
        [nameof(TimeAndScale)] = TimeAndScale,
        [nameof(SocialStructures)] = SocialStructures,
        [nameof(PhilosophicalQuestions)] = PhilosophicalQuestions,
        [nameof(NatureAndEnvironment)] = NatureAndEnvironment,
        [nameof(TechnologyAndMagic)] = TechnologyAndMagic,
        [nameof(EmotionalHooks)] = EmotionalHooks,
        //[nameof(Prompts)] = Prompts,
    };
}
