@Version 3

func fib(n: int) {
	let a = 0
	let b = 1
	let c = 0
	
	let i = 2
	while(i <= n) {
		i = i + 1

		c = a + b
		a = b
		b = c
	}

	API:assert(b, 55) # should be 55
}