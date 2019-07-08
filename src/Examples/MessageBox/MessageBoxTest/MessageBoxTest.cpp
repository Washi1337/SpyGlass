// MessageBoxTest.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <Windows.h>
#include <iostream>

int main()
{
    while (true) 
    {
        std::cout << "Press a key to show a message box" << std::endl;
        std::cin.ignore(100, '\n');
        MessageBoxA(NULL, "This is a message box", "This is the title", MB_ICONINFORMATION);
    }
}
