// SpyGlass.DummyTarget.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>

int DummyMethod(int a, int b, int c)
{
	std::cout << "This is some dummy method" << std::endl;
	std::cout << "A: " << std::hex << a << std::endl;
	std::cout << "B: " << std::hex << b << std::endl;
	std::cout << "C: " << std::hex << c << std::endl;
	
	return a + b + c;
}

int main()
{
	std::cout << "Address of DummyMethod: " << &DummyMethod << std::endl;

	while (true) 
	{
		std::cout << "Press enter to run DummyMethod(A: 1337, B: 1338, C: 1339)" << std::endl;
		std::cin.ignore(100, '\n');
		std::cout << "Result: " << DummyMethod(0x1337, 0x1338, 0x1339) << std::endl;
	}
}

