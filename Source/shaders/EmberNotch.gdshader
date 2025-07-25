shader_type canvas_item;

uniform vec4 fill_color : source_color = vec4(1.0,1.0,1.0,1.0);
uniform float fill_amount : hint_range(0.0, 1.0) = 1.0;
uniform float wave_strength : hint_range(0.0, 1.0) = 0.02;
uniform float wave_frequency : hint_range(1.0, 20.0) = 10.0;
uniform float wave_speed : hint_range(0.0, 10.0) = 2.0;
uniform float time;

void fragment(){
	vec2 uv = UV;
	
	// wave distortion at the top edge.
	float wave = sin(uv.x * wave_frequency * 2.0 * PI + time * wave_speed) * wave_strength;
	
	// where to place the wave.
	
	float threshold = 1.0 - fill_amount - wave;
	
	// get the texture and its uv's

	vec4 tex = texture(TEXTURE, uv);
	
	// only display pixels below the threshold and within visible alpha.
	if(uv.y < threshold || tex.a < 0.01){
		discard;
	}
	
	// set the color of the pixel.
	
	COLOR = tex * fill_color; // tint color.
}