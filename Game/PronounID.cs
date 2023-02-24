namespace Hypostasis.Game;

public enum PronounID : uint
{
    None = 0,
    // 1 - 8 were the subtarget commands from 1.0
    EnemyA = 9,
    EnemyB = 10,
    EnemyC = 11,
    EnemyD = 12,
    EnemyE = 13,
    EnemyF = 14,
    EnemyG = 15,
    EnemyH = 16,
    EnemyI = 17,
    EnemyJ = 18,
    EnemyK = 19,
    EnemyL = 20,
    EnemyM = 21,
    EnemyN = 22,
    EnemyO = 23,
    EnemyP = 24,
    EnemyQ = 25,
    EnemyR = 26,
    EnemyS = 27,
    EnemyT = 28,
    EnemyU = 29,
    EnemyV = 30,
    EnemyW = 31,
    EnemyX = 32,
    EnemyY = 33,
    EnemyZ = 34,
    // 35 - 42 may be the party's true order, while the next ones are based on the party list order
    P1 = 43, // Same as Me
    P2 = 44,
    P3 = 45,
    P4 = 46,
    P5 = 47,
    P6 = 48,
    P7 = 49,
    P8 = 50,
    // 51 - 58 are PROBABLY alliance 1
    // 59 - 66 are PROBABLY alliance 2
    E1 = 83,
    E2 = 84,
    E3 = 85,
    E4 = 86,
    E5 = 87,

    // These correspond to their row in the TextCommandParam sheet
    Target = 1000, // This is also the soft target if there is no target
    TargetsTarget = 1002,
    FocusTarget = 1004,
    LastTarget = 1006,
    LastAttacker = 1008,
    Anchor = 1010, // What is this? I don't know!
    MouseOver = 1012,
    Me = 1014,
    Companion = 1016, // AKA Chocobo AKA Buddy
    Pet = 1018,
    Reply = 1020,
    Attack1 = 1050,
    Attack2 = 1052,
    Attack3 = 1054,
    Attack4 = 1056,
    Attack5 = 1058,
    Bind1 = 1060,
    Bind2 = 1062,
    Bind3 = 1064,
    Stop1 = 1066,
    Stop2 = 1068,
    Square = 1070,
    Circle = 1072,
    Cross = 1074,
    Triangle = 1076,
    Attack = 1078,
    Bind = 1080,
    Stop = 1082,
    LastEnemy = 1084,
    Plus = 1116 // Alt name for Cross
    //E = 1118 // Don't know what this is
}