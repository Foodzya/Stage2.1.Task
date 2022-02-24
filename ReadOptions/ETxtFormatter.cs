﻿using System;
using FileReaderWriter.Menu;
using FileReaderWriter.Menu.MenuStates;

namespace FileReaderWriter.ReadOptions
{
    public class ETxtFormatter : IReadFormatter
    {
        public string FormatContent(string content)
        {
            return CipherDirectionMenu(content);
        }

        private string CipherDirectionMenu(string content)
        {
            Console.Clear();

            Console.WriteLine("Select encription direction:\n" +
            "1 -- Left\n" +
            "2 -- Right\n" +
            "3 -- Back to the menu\n");

            MenuContext menuContext = new MenuContext();
            CipherEncryptor encryptor = new CipherEncryptor();

            ConsoleKey direction = Console.ReadKey().Key;

            string result = string.Empty;

            int shift;

            switch (direction)
            {
                case ConsoleKey.D1:
                    shift = GetCipherShift();
                    result = encryptor.LeftShiftCipher(content, shift);
                    break;
                case ConsoleKey.D2:
                    shift = GetCipherShift();
                    result = encryptor.RightShiftCipher(content, shift);
                    break;
                case ConsoleKey.D3:
                    menuContext.ChangeMenuState(new ReadMenuState());
                    break;
                default: 
                    Console.WriteLine("An error occured");
                    CipherDirectionMenu(content);
                    break;                    
            }

            return result;
        }

        private int GetCipherShift()
        {
            MenuContext menuContext = new MenuContext();

            Console.WriteLine("\nCIPHER SHIFT MENU\n" +
            "Specify encription shift:\n" + 
            "Enter Q to go to the previous menu\n");
            
            string shiftInput = Console.ReadLine();

            if (shiftInput.ToLower() == "q")
                menuContext.ChangeMenuState(new ReadMenuState());
                
            int shift = 0;

            try 
            {
                shift = Int32.Parse(shiftInput);
            }
            catch(FormatException e)
            {
                Console.Clear();
                
                Console.WriteLine(e);

                GetCipherShift();
            }

            return shift;
        }
    }
}
