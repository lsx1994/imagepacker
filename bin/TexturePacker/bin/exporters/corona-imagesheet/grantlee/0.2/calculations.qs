/*
This solution for calculating values is not very nice but it works.
The set<variable> filters store the values in global variables
The calculate<value> filters do the calculations
*/

var height=0;
var width=0;
var sourceWidth=0;
var sourceHeight=0;

// store height of a sprite
var setHeight = function(input)
{
	height = parseFloat(input);
	return "";
};
setHeight.filterName = "setHeight";
setHeight.isSafe = false;
Library.addFilter("setHeight");

// store the width of a sprite
var setWidth = function(input)
{
	width = parseFloat(input);
	return "";
};
setWidth.filterName = "setWidth";
setWidth.isSafe = false;
Library.addFilter("setWidth");

// store sprite's original width
var setSourceWidth = function(input)
{
	sourceWidth = parseFloat(input);
	return "";
};
setSourceWidth.filterName = "setSourceWidth";
setSourceWidth.isSafe = false;
Library.addFilter("setSourceWidth");

// store sprite'S original height
var setSourceHeight = function(input)
{
	sourceHeight = parseFloat(input);
	return "";
};
setSourceHeight.filterName = "setSourceHeight";
setSourceHeight.isSafe = false;
Library.addFilter("setSourceHeight");

// calculate the sourceX, parameter is the offset of the trimmed sprite
var calculateSourceX = function(input)
{
	var x = parseFloat(input);
	return String(x + sourceWidth/2 - width/2);
};
calculateSourceX.filterName = "calculateSourceX";
calculateSourceX.isSafe = false;
Library.addFilter("calculateSourceX");

// calculate the sourceY, parameter is the offset of the trimmed sprite
var calculateSourceY = function(input)
{
	var y = parseFloat(input);
	return String(y + sourceHeight/2 - height/2);
};
calculateSourceY.filterName = "calculateSourceY";
calculateSourceY.isSafe = false;
Library.addFilter("calculateSourceY");
