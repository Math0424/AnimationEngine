@BlockId "TestCodeBlock"
@Version 2
@Author Math0424

func logic() {
	if (1 != 1) 
	{
		API.assert(1, 2) # failed test
	}
	else if(3 != 3)
	{
		API.assert(2, 3) # failed test
	}
	else 
	{
		return
	}
	API.assert(3, 4) # failed test
}


func loop() {
	var a = 10

	while(a != 0) {
		a = a - 1;
		API.log(a);
	}
}