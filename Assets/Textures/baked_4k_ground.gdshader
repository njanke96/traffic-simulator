shader_type spatial;
render_mode blend_mix,depth_draw_opaque,cull_back,diffuse_burley,specular_schlick_ggx;
uniform vec4 albedo : hint_color;
uniform sampler2D texture_albedo : hint_albedo;
uniform float specular;
uniform float metallic;
uniform float roughness : hint_range(0,1);
uniform float point_size : hint_range(0,128);
uniform sampler2D texture_normal : hint_normal;
uniform float normal_scale : hint_range(-16,16);
uniform float subsurface_scattering_strength : hint_range(0,1);
uniform sampler2D texture_subsurface_scattering : hint_white;
uniform vec3 uv1_scale;
uniform vec3 uv1_offset;
uniform vec3 uv2_scale;
uniform vec3 uv2_offset;


void vertex() {
	UV=UV*uv1_scale.xy+uv1_offset.xy;
}




void fragment() {
	vec2 base_uv = UV;
	vec4 albedo_tex = texture(texture_albedo,base_uv);
	
	// brightness and contrast
	vec3 color;
	color.rgb = mix(vec3(0.0), albedo.rgb * albedo_tex.rgb, 0.6);
	color.rgb = mix(vec3(0.5), color.rgb, 1.05);
	
	ALBEDO = color;
	METALLIC = metallic;
	ROUGHNESS = roughness;
	SPECULAR = specular;
	NORMALMAP = texture(texture_normal,base_uv).rgb;
	NORMALMAP_DEPTH = normal_scale;
	float sss_tex = texture(texture_subsurface_scattering,base_uv).r;
	SSS_STRENGTH=subsurface_scattering_strength*sss_tex;
}
