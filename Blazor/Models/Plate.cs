namespace Blazor.Models
{
    public class Plate
    {
        public string Id { get; set; } // ID from website
        
        public List<int> Row1 { get; set; } 
        public List<int> Row2 { get; set; } 
        public List<int> Row3 { get; set; } 

        // Constructor
        public Plate(string id, List<int> row1, List<int> row2, List<int> row3)
        {
            Id = id;
            Row1 = row1;
            Row2 = row2;
            Row3 = row3;
        }

        //Methods

        public bool CheckForOneRow()
        {
            if (Row1.Count == 0 || Row2.Count == 0 || Row3.Count == 0) 
                return true;
            return false;
        }

        public bool CheckForTwoRow() 
        { 
            if ((Row1.Count == 0 && Row2.Count == 0) ||
                (Row1.Count == 0 && Row3.Count == 0) ||
                (Row2.Count == 0 && Row3.Count == 0))
            {
                return true;
            }
            return false;
        }

        public bool CheckForFullPlate() 
        {
            if (Row1.Count == 0 && Row2.Count == 0 && Row3.Count == 0)
                return true;
            return false;
        }
        public void RemoveNumber(int number)
        {
            Row1.Remove(number);
            Row2.Remove(number);
            Row3.Remove(number);
        }

    }
}
