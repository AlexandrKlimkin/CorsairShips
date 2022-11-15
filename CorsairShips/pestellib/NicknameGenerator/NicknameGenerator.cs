using System;

namespace PestelLib
{
    public class NicknameGenerator
    {
        static string[] adjectives = { "Accidental", "Doubtful", "Main", "Achievable", "Elementarty", "Minor", "Advantageous", "Finger-printed", "Nasty", "Alcoholic",
            "Groundless", "Nutritious", "Animated", "Hard", "Obsolete", "Aquatic", "Harmful", "Optimal", "Aromatic", "High", "Organic",
            "Aspiring", "Honest", "Premium", "Bad", "Horrible", "Quizzical", "Bawdy", "Illegal", "Rainy", "Biographical", "Illegible",
            "Redundant", "Bizarre", "Imperfect", "Remarkable", "Broken", "Impossible", "Simple", "Careful", "Internal", "Tangible", "Credible",
            "Inventive", "Tricky", "Creepy", "Jazzy", "Wholesale", "Cumbersome", "Juvenile", "Worse", "Disastrous", "Legal", "Wry", "Dismissive",
            "Logical Lumpy", "Ablaze", "Distinct", "Quirky", "Adorable", "Drab", "Ruddy", "Alluring", "Dull", "Shiny", "Attractive", "Elegant",
            "Skinny", "Average", "Embarrassed", "Sloppy", "Awkward", "Fancy", "Smiling", "Balanced", "Fat", "Sparkling", "Beautiful", "Filthy",
            "Spotless", "Blonde", "Glamorous", "Strange", "Bloody", "Gleaming", "Tacky", "Blushing", "Glossy", "Tall", "Bright", "Graceful",
            "Thin", "Clean", "Grotesque", "Ugly", "Clear", "Handsome", "Unattractive", "Cloudy", "Homely", "Unbecoming", "Clumsy", "Interior",
            "Uncovered", "Colorful", "Lovely", "Unsightly", "Confident", "Magnificent", "Unusual", "Cracked", "Murky", "Watery", "Crooked",
            "Old-fashioned", "Weird", "Crushed", "Plain", "Wild", "Curly", "Poised", "Wiry", "Cute", "Pretty", "Wooden", "Debonair", "Puffy",
            "Worried", "Dirty", "Quaint", "Zaftig", "Aggressive", "Famous", "Restless", "Agoraphobic", "Fearless", "Rich", "Ambidextrous", "Fertile",
            "Righteous", "Ambitious", "Fragile", "Ritzy", "Amoral", "Frank", "Romantic", "Angelic", "Functional", "Rustic", "Brainy", "Gabby",
            "Ruthless", "Breathless", "Generous", "Sassy", "Busy", "Gifted", "Secretive", "Calm", "Helpful", "Sedate", "Capable", "Hesitant",
            "Shy", "Careless", "Innocent", "Sleepy", "Cautious", "Inquisitive", "Somber", "Cheerful", "Insane", "Stingy", "Clever", "Jaunty",
            "Stupid", "Common", "Juicy", "Super", "Complete", "Macho", "Swanky", "Concerned", "Manly", "Tame", "Crazy", "Modern", "Tawdry",
            "Curious", "Mushy", "Terrific", "Dead", "Naughty", "Testy", "Deep", "Odd", "Uninterested", "Delightful", "Old", "Vague", "Determined",
            "Open", "Verdant", "Different", "Outstanding", "Vivacious", "Diligent", "Perky", "Wacky", "Energetic", "Poor", "Wandering", "Erratic",
            "Powerful", "Wild", "Evil", "Puzzled", "Womanly", "Exuberant", "Real", "Wrong", "Abrasive", "Embarrassed", "Grumpy", "Abrupt", "Energetic",
            "Kind", "Afraid", "Enraged", "Lazy", "Agreeable", "Enthusiastic", "Lively", "Aggressive", "Envious", "Lonely", "Amiable", "Evil",
            "Lucky", "Amused", "Excited", "Mad", "Angry", "Exhausted", "Manic", "Annoyed", "Exuberant", "Mysterious", "Ashamed", "Fair", "Nervous",
            "Bad", "Faithful", "Obedient", "Bitter", "Fantastic", "Obnoxious", "Bewildered", "Fierce", "Outrageous", "Boring", "Fine", "Panicky",
            "Brave", "Foolish", "Perfect", "Callous", "Frantic", "Persuasive", "Calm", "Friendly", "Pleasant", "Calming", "Frightened", "Proud",
            "Charming", "Funny", "Quirky", "Cheerful", "Furious", "Relieved", "Combative", "Gentle", "Repulsive", "Comfortable", "Glib", "Rundown",
            "Defeated", "Glorious", "Sad", "Confused", "Good", "Scary", "Cooperative", "Grateful", "Selfish", "Courageous", "Grieving", "Silly",
            "Cowardly", "Gusty", "Splendid", "Crabby", "Gutless", "Successful", "Creepy", "Happy", "Tedious", "Cross", "Healthy", "Tense",
            "Cruel", "Heinous", "Terrible", "Dangerous", "Helpless", "Thankful", "Defeated", "Hilarious", "Thoughtful", "Defiant", "Hungry",
            "Thoughtless", "Delightful", "Hurt", "Tired", "Depressed", "Hysterical", "Troubled", "Determined", "Immoral", "Upset", "Disgusted",
            "Impassioned", "Weak", "Disturbed", "Indignant", "Weary", "Eager", "Irate", "Wicked", "Elated", "Itchy", "Worried", "Embarrassed",
            "Jealous", "Zany", "Enchanting", "Jolly", "Zealous", "Heavy", "One", "Ample", "Hundreds", "Paltry", "Astronomical", "Large",
            "Plentiful", "Bountiful", "Light", "Profuse", "Considerable", "Limited", "Several", "Copious", "Little", "Sizable", "Countless", "Many",
            "Some", "Each", "Measly", "Sparse", "Enough", "Mere", "Substantial", "Every", "Multiple", "Teeming", "Few", "Myriad", "Ten", "Full",
            "Numerous", "Very", "Annual", "Futuristic", "Rapid", "Brief", "Historical", "Regular", "Daily", "Irregular", "Short", "Early", "Late",
            "Slow", "Eternal", "Long", "Speed", "Fast", "Modern", "Speedy", "First", "Old", "Swift", "Fleet", "Old-fashioned", "Waiting", "Future",
            "Quick", "Young", "Blobby", "Distorted", "Rotund", "Broad", "Flat", "Round", "Chubby", "Fluffy", "Skinny", "Circular", "Globular", "Square",
            "Crooked", "Hollow", "Steep", "Curved", "Low", "Straight", "Cylindrical", "Narrow", "Triangular", "Deep", "Oval", "Wide", "Abundant",
            "Jumbo", "Puny", "Big-boned", "Large", "Scrawny", "Chubby", "Little", "Short", "Fat", "Long", "Small", "Giant", "Majestic", "Tall",
            "Gigantic", "Mammoth", "Teeny", "Great", "Massive", "Thin", "Huge", "Miniature", "Tiny", "Immense", "Petite", "Vast", "Azure", "Gray",
            "Pinkish", "Black", "Green", "Purple", "Blue", "Indigo", "Red", "Bright", "Lavender", "Rosy", "Brown", "Light", "Scarlet", "Crimson",
            "Magenta", "Silver", "Dark", "Multicolored", "Turquoise", "Drab", "Mustard", "Violet", "Dull", "Orange", "White", "Gold", "Pink",
            "Yellow", "Blaring", "Melodic", "Screeching", "Deafening", "Moaning", "Shrill", "Faint", "Muffled", "Silent", "Hoarse", "Mute", "Soft",
            "High-pitched", "Noisy", "Squealing", "Hissing", "Purring", "Squeaking", "Hushed", "Quiet", "Thundering", "Husky", "Raspy", "Voiceless",
            "Loud", "Resonant", "Whispering", "Boiling", "Fluffy", "Sharp", "Breezy", "Freezing", "Silky", "Bumpy", "Fuzzy", "Slick", "Chilly", "Greasy",
            "Slimy", "Cold", "Hard", "Slippery", "Cool", "Hot", "Smooth", "Cuddly", "Icy", "Soft", "Damp", "Loose", "Solid", "Dirty", "Melted", "Sticky",
            "Dry", "Painful", "Tender", "Dusty", "Prickly", "Tight", "Encrusted", "Rough", "Uneven", "Filthy", "Shaggy", "Warm", "Flaky", "Shaky",
            "Wet", "Bitter", "Lemon-flavored", "Spicy", "Bland", "Minty", "Sweet", "Delicious", "Pickled", "Tangy", "Fruity", "Salty", "Tasty", "Gingery",
            "Sour", "Yummy", };

        static string[] nouns = {"Alligator", "Crocodile", "Alpaca", "Ant", "Antelope", "Ape", "Armadillo", "Donkey", "Baboon", "Badger", "Bat",
            "Bear", "Beaver", "Bee", "Beetle", "Buffalo", "Butterfly", "Camel", "Carabao", "Caribou", "Cat", "Cattle",
            "Cheetah", "Chimpanzee", "Chinchilla", "Cicada", "Clam", "Cockroach", "Cod", "Coyote", "Crab", "Cricket",
            "Crow", "Deer", "Dinosaur", "Dog", "Dolphin", "Duck", "Eel", "Elephant", "Elk", "Ferret", "Fish", "Fly",
            "Fox", "Frog", "Gerbil", "Giraffe", "Gnat", "Gnu", "Goat", "Goldfish", "Gorilla", "Grasshopper", "Guinea", "Hamster",
            "Hare", "Hedgehog", "Herring", "Hippopotamus", "Hornet", "Horse", "Hound", "Hyena", "Impala", "Insect", "Jackal", "Jellyfish",
            "Kangaroo", "Koala", "Leopard", "Lion", "Lizard", "Llama", "Locust", "Louse", "Mallard", "Mammoth", "Manatee", "Marten",
            "Mink", "Minnow", "Mole", "Monkey", "Moose", "Mosquito", "Mouse", "Rat", "Mule", "Muskrat", "Otter", "Ox",
            "Oyster", "Panda", "Pig", "Platypus", "Porcupine", "Prairie", "Pug", "Rabbit", "Raccoon", "Reindeer", "Rhinoceros", "Salmon",
            "Sardine", "Scorpion", "Seal", "Serval", "Shark", "Sheep", "Skunk", "Snail", "Snake", "Spider", "Squirrel", "Termite",
            "Tiger", "Trout", "Turtle", "Walrus", "Wasp", "Weasel", "Whale", "Wolf", "Wombat", "Woodchuck", "Worm", "Yak",
            "Zebra", "Accountant", "Actor", "Advocate", "Ambassador", "Apothecary", "Appraiser", "Architect", "Artist", "Astrologer", "Astronomer", "Attender",
            "Auctioneer", "Auditor", "Author", "Baker", "Banker", "Barber", "Beggar", "Bird Catcher", "Blacksmith", "Boatman", "Booking Clerk", "Boxer",
            "Brick Layer", "Broker", "Bursar", "Butcher", "Butler", "Capitalist", "Carpenter", "Cashier", "Chaprassi", "Chauffer", "Chef", "Chemist",
            "Cleaner", "Clergyman", "Clerk", "Cobbler", "Compositor", "Compounder", "Conductor", "Constable", "Cook", "Coolie", "Dancer", "Dealer",
            "Debtor", "Dentist", "Dhoby", "Doctor", "Draughtsman", "Driver", "Druggist", "Drummer", "Dyer", "Editor", "Engineer", "Examiner",
            "Executor", "Farmer", "Fisherman", "Gardener", "Gate-Keeper", "Goldsmith", "Grass-Cutter", "Grocer", "Hawker", "Horseman", "Hunter", "Inspector",
            "Jeweller", "Joiner", "Labourer", "Marker", "Mason", "Mechanic", "Mediator", "Merchant", "Midwife", "Milkman", "Minister", "Money-Lender",
            "Mortgager", "Musician", "Navigator", "Novelist", "Nurse", "Observer", "Officer", "Oil-Monger", "Operator", "Overseer", "Painter", "Payee",
            "Peon", "Photographer", "Pilot", "Planter", "Playwright", "Poet", "Policeman", "Porter", "Post Master", "Postman", "Pot Maker", "Potter",
            "Priest", "Printer", "Publisher", "Puill-Driver", "Purohit", "Reader", "Registrar", "Repairer", "Reporter", "Rower", "Scavenger", "Scullion",
            "Secretary", "Shoemaker", "Shop-Keeper", "Shroff", "Signaller", "Silversmith", "Singer", "Soldier", "Sorcerer", "Sorter", "Speaker", "Stenographer",
            "Superintendent", "Supervisor", "Surgeon", "Surveyor", "Tailor", "Teacher", "Tinker", "Translator", "Tutor", "Typist", "Usurer", "Waiter",
            "Washer Man", "Watchman", "Weaver", "Weigh Man", "Writer", "Shaman"};

        private static string[] _usernamesDatabase = null;
        private static Random rnd = new Random();

        public static string GenerateName()
        {
            if (_usernamesDatabase == null)
            {
                var text = ResourcesHelper.GetEmbeddedResource(System.Reflection.Assembly.GetExecutingAssembly(), "NicknameGenerator.usernamesdatabase.txt");
                _usernamesDatabase = text.Split('\n');
            }

            var dice = rnd.NextDouble();

            if (dice > 0.85)
            {
                return adjectives[rnd.Next(0, adjectives.Length)] + nouns[rnd.Next(0, nouns.Length)] +
                       rnd.Next(10, 9999).ToString();
            }
            else if (dice > 0.05f)
            {
                return _usernamesDatabase[rnd.Next(0, _usernamesDatabase.Length)];
            }
            else
            {
                return "Bot" + rnd.Next(10, 9999);
            }
        }
    }
}
