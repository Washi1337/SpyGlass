// SpyGlass.DummyTarget.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>

void DummyMethod(int a, int b, int c)
{
	std::cout << "This is some dummy method" << std::endl;
	std::cout << "A: " << std::hex << a << std::endl;
	std::cout << "B: " << std::hex << b << std::endl;
	std::cout << "C: " << std::hex << c << std::endl;
}

int main()
{
	std::cout << "DummyMethod: " << &DummyMethod << std::endl;

	while (true) 
	{
		std::cout << "Press enter to run DummyMethod" << std::endl;
		std::cin.ignore(100, '\n');
		DummyMethod(0x1337, 0x1338, 0x1339);
	}
}

