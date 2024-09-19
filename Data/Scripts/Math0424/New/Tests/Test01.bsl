@Author Math
@Version 3

import "LinkedFile.bsl"

using light1 as Light()
using subpart1 as Subpart("name")
using subgrid1 as Grid("name", 1) parent subpart1

# this is a comment
let autoValue = "string"

[Synced(True, False)]
let stringValue = "This is \n a string"

let intValue: int = 1
let floatValue: float = 1.0
let boolValue: bool = True

struct Thinking {
	value: string
	value2: int
	value4: float
}

[Listener("Block/Built")]
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

	value = 10 + 43
	FunctionCall(va, ue + 12 - 10 / 20)
	value = (10 + 2 - 199.0) / FunctionCall(value)
	API:log(x)
	object.call(va, lue).rotate(10).explode(450 + 328 / FunctionCall(20, 29, 3 + 2))
	
	let structDef: Thinking = default
	let timeOfDay: float = API:TimeOfDay()



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
