@BlockID "LargeBlockSmallAtmosphericThrust"
@Version 2
@Author Math0424

using outer_0 as Subpart("Plate0")
using outer_1 as Subpart("Plate1")
using outer_2 as Subpart("Plate2")
using outer_3 as Subpart("Plate3")
using outer_4 as Subpart("Plate4")
using outer_5 as Subpart("Plate5")
using outer_6 as Subpart("Plate6")
using outer_7 as Subpart("Plate7")
using outer_8 as Subpart("Plate8")
using outer_9 as Subpart("Plate9")
using outer_10 as Subpart("Plate10")
using outer_11 as Subpart("Plate11")

using inner_1 as Subpart("PlateSmall0")
using inner_2 as Subpart("PlateSmall1")
using inner_3 as Subpart("PlateSmall2")
using inner_4 as Subpart("PlateSmall3")

var oldVal = 0
var value = 0

func SetPosition() {
	value = Block.CurrentThrustPercent()
	var setVal = oldVal - value;
	oldVal = value

	inner_1.rotate([-1, 0, 0], setVal * 9, 20, Linear)
	inner_2.rotate([0, 1, 0], setVal * 9, 20, Linear)
 	inner_3.rotate([1, 0, 0], setVal * 9, 20, Linear)
	inner_4.rotate([0, -1, 0], setVal * 9, 20, Linear)

	outer_0.rotate([-1, 0, 0], setVal * 9, 20, Linear)
	outer_1.rotate([-0.86, 0.5, 0], setVal * 9, 20, Linear)
	outer_2.rotate([-0.5, 0.86, 0], setVal * 9, 20, Linear)
	outer_3.rotate([0, 1, 0], setVal * 9, 20, Linear)
	outer_4.rotate([0.5, 0.86, 0], setVal * 9, 20, Linear)
	outer_5.rotate([0.86, 0.5, 0], setVal * 9, 20, Linear)
    outer_6.rotate([1, 0, 0], setVal * 9, 20, Linear)
	outer_7.rotate([0.86, -0.5, 0], setVal * 9, 20, Linear)
	outer_8.rotate([0.5, -0.86, 0], setVal * 9, 20, Linear)
	outer_9.rotate([0, -1, 0], setVal * 9, 20, Linear)
	outer_10.rotate([-0.5, -0.86, 0], setVal * 9, 20, Linear)
	outer_11.rotate([-0.86, -0.5, 0], setVal * 9, 20, Linear)
}

action block() {
	create() {
		oldVal = Block.CurrentThrustPercent()
		api.startLoop("SetPosition", 20, -1)
	}
    built() {
		oldVal = Block.CurrentThrustPercent()
    }
}
