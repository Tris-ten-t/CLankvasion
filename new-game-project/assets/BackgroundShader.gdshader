shader_type canvas_item;

uniform float vignette_strength : hint_range(0.0, 1.0) = 0.4;
uniform float vignette_size : hint_range(0.0, 1.0) = 0.6;
uniform vec4 tint_color : source_color = vec4(1.0, 1.0, 1.0, 1.0);

void fragment() {
    // Get the original texture color
    vec4 tex_color = texture(TEXTURE, UV) * tint_color;

    // Calculate vignette
    vec2 uv = UV - vec2(0.5, 0.5);
    float vignette = length(uv) / vignette_size;
    vignette = pow(vignette, 2.0) * vignette_strength;
    vignette = clamp(vignette, 0.0, 1.0);

    // Apply vignette
    tex_color.rgb -= vignette;

    COLOR = tex_color;
}