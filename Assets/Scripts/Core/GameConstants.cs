public static class GameConstants
{
    public const int MAX_TURNS = 60;
    public const int TURNS_PER_YEAR = 12;
    public const int MAP_WIDTH = 24;
    public const int MAP_HEIGHT = 24;

    public const int STARTING_FOOD = 1000;
    public const int STARTING_FUNDS = 500;
    public const int STARTING_POPULATION = 300;
    public const int FOOD_CONSUMPTION_PER_POP = 1;

    public const int STARTING_F_STONE = 80;
    public const int STARTING_F_WOOD = 100;
    public const int STARTING_F_METAL = 40;
    public const int STARTING_F_FOODSTUFF = 60;
    public const int STARTING_F_MAGIC = 20;

    public const int INITIAL_GUILD_MEMBER_COUNT = 5;
    public const int INITIAL_CITY_DEFENSE = 30;

    public const int FUNDS_DEFICIT_DEFEAT_TURNS = 3;
    public const int HAPPINESS_CRISIS_DEFEAT_TURNS = 3;

    public const int FIRST_RAID_START_TURN = 6;
    public const int FIRST_RAID_FORCED_AFTER_TURNS = 4;
    public const int FIRST_RAID_POWER_UNEXPLORED = 100;
    public const int FIRST_RAID_POWER_EXPLORED = 75;
    public const int RAID_ORIGIN_MAX_DISTANCE = 5;
    public const int EXPLORATION_FAILURE_PROGRESS = 34;
    public const int EXPLORATION_SUCCESS_PROGRESS = 50;
    public const float EXPLORATION_SUCCESS_RATE = 0.6f;

    public const int DUNGEON_FLOOR_COUNT = 5;
    public const int DUNGEON_TURNS_PER_FLOOR = 3;

    public const float SUBSEQUENT_RAID_BASE_PROBABILITY = 0.2f;
    public const int SUBSEQUENT_RAID_POWER_BASE = 80;
    public const int SUBSEQUENT_RAID_POWER_PER_TURN = 2;

    public const int BASE_HAPPINESS = 50;
    public const float FOOD_RATIO_GOOD_THRESHOLD = 3.0f;
    public const float FOOD_RATIO_BAD_THRESHOLD = 1.0f;
    public const int FOOD_SUFFICIENCY_MAX_BONUS = 15;
    public const int FOOD_SHORTAGE_MAX_PENALTY = -25;
    public const int ENTERTAINMENT_BONUS_PER_LEVEL = 3;
    public const float OVERCROWDING_START_RATIO = 0.9f;
    public const int OVERCROWDING_MAX_PENALTY = -20;
    public const int FUNDS_NEGATIVE_PENALTY = -10;
}
