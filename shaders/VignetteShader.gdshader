shader_type canvas_item;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;

uniform float vignette_intensity = 0.4;
uniform float vignette_opacity : hint_range(0.0, 1.0) = 0.5;
uniform vec4 vignette_rgb = vec4(0.0, 0.0, 0.0, 1.0);

float vignette(vec2 uv){
	uv *= 1.0 - uv.xy;
	float v = uv.x * uv.y * 15.0;
	return pow(v, vignette_intensity * vignette_opacity);
}

void fragment(){
	vec4 color = texture(SCREEN_TEXTURE, SCREEN_UV);
	vec4 text = texture(TEXTURE, UV);
	
	text *= vignette_rgb;
	text *= (1.0 - vignette(UV));
	
	COLOR = vec4(text.rgb * color.rgb, text.a);
}

