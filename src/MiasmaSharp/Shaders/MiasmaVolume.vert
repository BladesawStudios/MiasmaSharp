#version 330 core

// The box around a gloom volume's unit volume, for the shape pass. Drawn inside out with no depth
// test, so it covers the screen wherever any of the volume does, the camera inside it included.

layout(location = 0) in vec3 aCorner;

// The unit cube, |p| <= 0.5, to clip space: projection, view and the volume's world matrix.
uniform mat4 uUnitToClip;

void main()
{
    gl_Position = uUnitToClip * vec4(aCorner, 1.0);
}
