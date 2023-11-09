@BlockId "TestCodeBlock"
@Version 2
@Author Math0424

func fib(n) {
	var a = 0
	var b = 1
	var c = 0
	
	var i = 2
	while(i <= n) {
		i = i + 1

		c = a + b
		a = b
		b = c
	}

	API.assert(b, 55) # should be 55
}