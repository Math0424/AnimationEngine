@BlockId "TestCodeBlock"
@Version 2
@Author Math0424

func logic() {
	if(true == true && true == true)
	{
		return
	}

	API.assert(0, 1)
	#if (1 != 1) 
	#{
	#	API.assert(1, 2) # failed test
	#}
	#else if(3 != 3 || 1 == 2 || 1 == 2 || 1 == 2)
	#{
	#	API.assert(2, 3) # failed test
	#}
	#else 
	#{
	#	return
	#}
	#API.assert(3, 4) # failed test
}

#func loop() {
#	var a = 10
#
#	while(a != 0) {
#		a = a - 1;
#		API.log(a);
#	}
#}