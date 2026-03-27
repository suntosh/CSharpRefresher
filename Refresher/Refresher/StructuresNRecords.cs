

namespace Refresher
{

    public class UserClass
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public record UserRecord(string Name, int Age);

    public struct CoordStruct
    {
        public int X;
        public int Y;
    }

    public record struct CoordRecord(int X, int Y);

    public static class StructuresNRecords
    {

        public static void Exec()
        {
            var c1 = new UserClass { Name = "Santosh", Age = 50 };
            var c2 = new UserClass { Name = "Santosh", Age = 50 };
            Console.WriteLine(c1 == c2); // False

            var r1 = new UserRecord("Santosh", 50);
            var r2 = new UserRecord("Santosh", 50);
            Console.WriteLine(r1 == r2); // True

            var s1 = new CoordStruct { X = 1, Y = 2 };
            var s2 = s1;
            s2.X = 99;
            Console.WriteLine(s1.X); // 1

            var rs1 = new CoordRecord(1, 2);
            var rs2 = new CoordRecord(1, 2);
            Console.WriteLine(rs1 == rs2); // True
        }
    }
}
