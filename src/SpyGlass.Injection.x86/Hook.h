#pragma once

#include <vector>

struct HookParameters
{
	void* Address;
	int BytesToOverwrite;
	std::vector<int> OffsetsNeedingFixup;
};

typedef void (_stdcall *HookCallBack)(SIZE_T* registers, SIZE_T* stack);

class Hook
{
public:
	Hook(HookParameters parameters, HookCallBack callback);
	~Hook();

	void Set();
	void Unset();

private:
	void ReadBytesToOverwrite();
	void CreateTrampoline();
	void CreateHookBytes();

	HookParameters _parameters;
	HookCallBack _callback;

	bool _isSet;

	char* _originalBytes;
	char* _hookBytes;
	void* _trampoline;
};

#define REGISTER_EIP 7
#define REGISTER_EAX 6
#define REGISTER_ECX 5
#define REGISTER_EDX 4
#define REGISTER_EBX 3
#define REGISTER_EBP 2
#define REGISTER_ESI 1
#define REGISTER_EDI 0