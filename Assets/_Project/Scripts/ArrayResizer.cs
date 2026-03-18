using UnityEngine;

namespace _Project.Scripts
{
    public class ArrayResizer : MonoBehaviour
    {
        private int[] numberArray; // The array to resize

        void Start()
        {
            // Initialize the array with a starting size of 3
            numberArray = new int[] { 1, 2, 3 };
            PrintArray("Initial array");

            // Resize the array to a new size
            ResizeArray(5); // Increase size to 5
            PrintArray("After resizing to 5");

            // Resize it again to a smaller size
            ResizeArray(2); // Decrease size to 2
            PrintArray("After resizing to 2");
        }

        void ResizeArray(int newSize)
        {
            // Create a new array with the desired size
            int[] newArray = new int[newSize];

            // Copy the old array's data into the new array (up to the smaller of the two sizes)
            for (int i = 0; i < Mathf.Min(numberArray.Length, newSize); i++)
            {
                newArray[i] = numberArray[i];
            }

            // Replace the old array with the new one
            numberArray = newArray;
        }

        void PrintArray(string message)
        {
            string arrayContents = string.Join(", ", numberArray);
            Debug.Log($"{message}: [{arrayContents}] (Size: {numberArray.Length})");
        }
    }
}