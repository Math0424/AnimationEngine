@Author Math
@Version 3

# this is a comment

let autoValue = "script"

[Synced(True, False)]
let stringValue = "This is \n a string"

let intValue: int = 1
let floatValue: float = 1.0
let boolValue: bool = True

[MethodHeader0]
[MethodHeader1(10, 10)]
[MethodHeader2(10, 12)]
func MyCoolFunction01(xValue: string, yValue: float, zValue: int) {

	let intValue: int = 1
	let floatValue: float = 1.0
	let boolValue = True

	if (1 + 2 == 3 || 2 > 3 || !(boolValue > 1)) {
		let boolValue2 = True
		break
	}

	while (True) {
		let boolValue2 = True
		return 1
	}

	# value = 10
	# FunctionCall(va, lue)
	# object.call(va, lue)
	# API:log(x)


	# TODO in the future
	# switch (value) {
	# 	  case 1 {
	# 	  
	# 	  }
	# 	  case 2
	# 	  case 3 {
	# 	  
	# 	  }
	# }
	# keyword[value] = 10
	# let arrayValue: [int, 3] = [1, 2, 3]
}

struct Thinking {
	value: string
	value2: int
	value4: float
}