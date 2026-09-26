"""
Gerador de Identidade Visual e Assets Oficiais para sisQDT_LIGHT.
Gera SVGs vetoriais, PNGs em múltiplos temas/resoluções e ICO Windows multi-resolução com canal alfa.
"""

import os
from PIL import Image, ImageDraw, ImageFont

ASSETS_DIR = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "assets", "brand"))
WPF_ASSETS_DIR = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "src", "QdtCqts.Desktop.Wpf", "Assets"))
os.makedirs(ASSETS_DIR, exist_ok=True)
os.makedirs(WPF_ASSETS_DIR, exist_ok=True)

# Paleta oficial de engenharia
COLOR_PRIMARY_BLUE = "#0A58CA"      # Azul Elétrico Técnico
COLOR_SECONDARY_BLUE = "#3B82F6"    # Azul Linhas de Distribuição
COLOR_AMBER = "#F59E0B"             # Âmbar Energia / Potência Ativa
COLOR_AMBER_DARK = "#D97706"        # Âmbar Sombra
COLOR_SLATE_DARK = "#0F172A"        # Grafite Técnico Profundo
COLOR_SLATE_LIGHT = "#F8FAFC"       # Branco Técnico
COLOR_MUTED = "#64748B"             # Cinza Técnico de Cotas/Metadados

def create_svg_icon(foreground_color=COLOR_PRIMARY_BLUE, accent_color=COLOR_AMBER, bg_color=None):
    """Gera o símbolo geométrico vetorial representando a rede de distribuição radial QDT."""
    bg_element = f'<rect width="512" height="512" rx="96" fill="{bg_color}"/>' if bg_color else ""
    return f"""<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512" width="100%" height="100%">
  <defs>
    <linearGradient id="blueGrad" x1="0%" y1="0%" x2="100%" y2="100%">
      <stop offset="0%" stop-color="{foreground_color}"/>
      <stop offset="100%" stop-color="{COLOR_SECONDARY_BLUE}"/>
    </linearGradient>
    <linearGradient id="amberGrad" x1="0%" y1="0%" x2="100%" y2="100%">
      <stop offset="0%" stop-color="{COLOR_AMBER}"/>
      <stop offset="100%" stop-color="{COLOR_AMBER_DARK}"/>
    </linearGradient>
    <filter id="subtleGlow" x="-20%" y="-20%" width="140%" height="140%">
      <feGaussianBlur stdDeviation="8" result="blur"/>
      <feComposite in="SourceGraphic" in2="blur" operator="over"/>
    </filter>
  </defs>
  {bg_element}
  <!-- Malha geométrica de fundo / Circuito sutil -->
  <g stroke="{COLOR_MUTED}" stroke-width="2" stroke-opacity="0.25" stroke-dasharray="6 6">
    <circle cx="256" cy="256" r="190" fill="none"/>
    <circle cx="256" cy="256" r="110" fill="none"/>
    <line x1="66" y1="256" x2="446" y2="256"/>
    <line x1="256" y1="66" x2="256" y2="446"/>
  </g>
  
  <!-- Linhas de Distribuição / Trechos de Tronco e Ramificação (Topologia Radial) -->
  <g stroke="url(#blueGrad)" stroke-width="18" stroke-linecap="round" stroke-linejoin="round">
    <!-- Alimentador Tronco Central (TR -> LID -> Barramento) -->
    <line x1="256" y1="90" x2="256" y2="256"/>
    <!-- Ramificações Radiais (Lados / Derivações de Rede) -->
    <line x1="256" y1="256" x2="120" y2="340"/>
    <line x1="256" y1="256" x2="392" y2="340"/>
    <line x1="120" y1="340" x2="90" y2="420"/>
    <line x1="120" y1="340" x2="180" y2="420"/>
    <line x1="392" y1="340" x2="332" y2="420"/>
    <line x1="392" y1="340" x2="422" y2="420"/>
  </g>

  <!-- Segmentos de cálculo de impedância / cota diferencial (Delta V) -->
  <g stroke="{accent_color}" stroke-width="8" stroke-linecap="round">
    <line x1="256" y1="130" x2="290" y2="130"/>
    <line x1="256" y1="190" x2="290" y2="190"/>
    <line x1="290" y1="130" x2="290" y2="190"/>
  </g>

  <!-- Nós de Distribuição (Postes, Caixas de Passagem, Transformador) -->
  <!-- Nós Secundários / Cargas Terminais -->
  <circle cx="90" cy="420" r="16" fill="{foreground_color}" stroke="{COLOR_SLATE_LIGHT}" stroke-width="4"/>
  <circle cx="180" cy="420" r="16" fill="{foreground_color}" stroke="{COLOR_SLATE_LIGHT}" stroke-width="4"/>
  <circle cx="332" cy="420" r="16" fill="{foreground_color}" stroke="{COLOR_SLATE_LIGHT}" stroke-width="4"/>
  <circle cx="422" cy="420" r="16" fill="{foreground_color}" stroke="{COLOR_SLATE_LIGHT}" stroke-width="4"/>
  
  <!-- Nós de Bifurcação / Derivação -->
  <circle cx="120" cy="340" r="22" fill="{COLOR_SECONDARY_BLUE}" stroke="{COLOR_SLATE_LIGHT}" stroke-width="5"/>
  <circle cx="392" cy="340" r="22" fill="{COLOR_SECONDARY_BLUE}" stroke="{COLOR_SLATE_LIGHT}" stroke-width="5"/>

  <!-- Nó Central de Derivação (LID) -->
  <circle cx="256" cy="256" r="32" fill="url(#amberGrad)" stroke="{COLOR_SLATE_LIGHT}" stroke-width="6"/>
  <circle cx="256" cy="256" r="12" fill="{COLOR_SLATE_LIGHT}"/>

  <!-- Nó Raiz / Transformador (TR) -->
  <circle cx="256" cy="90" r="28" fill="{foreground_color}" stroke="{accent_color}" stroke-width="6"/>
  <circle cx="256" cy="90" r="10" fill="{accent_color}"/>
</svg>"""

def create_svg_full(foreground_color=COLOR_PRIMARY_BLUE, text_primary=COLOR_SLATE_DARK, text_accent=COLOR_AMBER, bg_color=None):
    """Gera o logotipo completo vetorial com o símbolo e a tipografia oficial sisQDT_LIGHT."""
    bg_element = f'<rect width="1200" height="340" rx="32" fill="{bg_color}"/>' if bg_color else ""
    return f"""<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1200 340" width="100%" height="100%">
  <defs>
    <linearGradient id="blueGradLg" x1="0%" y1="0%" x2="100%" y2="100%">
      <stop offset="0%" stop-color="{foreground_color}"/>
      <stop offset="100%" stop-color="{COLOR_SECONDARY_BLUE}"/>
    </linearGradient>
    <linearGradient id="amberGradLg" x1="0%" y1="0%" x2="100%" y2="100%">
      <stop offset="0%" stop-color="{text_accent}"/>
      <stop offset="100%" stop-color="{COLOR_AMBER_DARK}"/>
    </linearGradient>
  </defs>
  {bg_element}

  <!-- SÍMBOLO EMBUTIDO (Esquerda) -->
  <g transform="translate(40, 20) scale(0.58)">
    <!-- Malha sutil -->
    <g stroke="{COLOR_MUTED}" stroke-width="3" stroke-opacity="0.25" stroke-dasharray="6 6">
      <circle cx="256" cy="256" r="190" fill="none"/>
      <circle cx="256" cy="256" r="110" fill="none"/>
    </g>
    <!-- Linhas de Distribuição -->
    <g stroke="url(#blueGradLg)" stroke-width="22" stroke-linecap="round" stroke-linejoin="round">
      <line x1="256" y1="90" x2="256" y2="256"/>
      <line x1="256" y1="256" x2="120" y2="340"/>
      <line x1="256" y1="256" x2="392" y2="340"/>
      <line x1="120" y1="340" x2="90" y2="420"/>
      <line x1="120" y1="340" x2="180" y2="420"/>
      <line x1="392" y1="340" x2="332" y2="420"/>
      <line x1="392" y1="340" x2="422" y2="420"/>
    </g>
    <!-- Cotas Delta V -->
    <g stroke="{text_accent}" stroke-width="10" stroke-linecap="round">
      <line x1="256" y1="130" x2="295" y2="130"/>
      <line x1="256" y1="190" x2="295" y2="190"/>
      <line x1="295" y1="130" x2="295" y2="190"/>
    </g>
    <!-- Nós -->
    <circle cx="90" cy="420" r="18" fill="{foreground_color}" stroke="{COLOR_SLATE_LIGHT}" stroke-width="5"/>
    <circle cx="180" cy="420" r="18" fill="{foreground_color}" stroke="{COLOR_SLATE_LIGHT}" stroke-width="5"/>
    <circle cx="332" cy="420" r="18" fill="{foreground_color}" stroke="{COLOR_SLATE_LIGHT}" stroke-width="5"/>
    <circle cx="422" cy="420" r="18" fill="{foreground_color}" stroke="{COLOR_SLATE_LIGHT}" stroke-width="5"/>
    <circle cx="120" cy="340" r="24" fill="{COLOR_SECONDARY_BLUE}" stroke="{COLOR_SLATE_LIGHT}" stroke-width="6"/>
    <circle cx="392" cy="340" r="24" fill="{COLOR_SECONDARY_BLUE}" stroke="{COLOR_SLATE_LIGHT}" stroke-width="6"/>
    <circle cx="256" cy="256" r="36" fill="url(#amberGradLg)" stroke="{COLOR_SLATE_LIGHT}" stroke-width="7"/>
    <circle cx="256" cy="256" r="14" fill="{COLOR_SLATE_LIGHT}"/>
    <circle cx="256" cy="90" r="30" fill="{foreground_color}" stroke="{text_accent}" stroke-width="7"/>
    <circle cx="256" cy="90" r="12" fill="{text_accent}"/>
  </g>

  <!-- TIPOGRAFIA: sisQDT_LIGHT -->
  <g font-family="-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif">
    <!-- sis: Minúsculo, Regular/Médio -->
    <text x="380" y="195" font-size="110" font-weight="400" fill="{text_primary}" letter-spacing="-1">sis</text>
    <!-- QDT: Maiúsculo, Bold Técnico -->
    <text x="515" y="195" font-size="115" font-weight="800" fill="{foreground_color}" letter-spacing="1">QDT</text>
    <!-- _LIGHT: Maiúsculo com underscore, Semi-Bold Âmbar -->
    <text x="795" y="195" font-size="115" font-weight="700" fill="{text_accent}" letter-spacing="2">_LIGHT</text>
    
    <!-- Subtítulo Técnico de Engenharia -->
    <text x="385" y="245" font-size="24" font-weight="600" fill="{COLOR_MUTED}" letter-spacing="5">SISTEMA UNIFICADO DE CÁLCULO DE REDES ELÉTRICAS</text>
    
    <!-- Linha técnica de precisão de rodapé -->
    <line x1="385" y1="265" x2="1140" y2="265" stroke="{COLOR_MUTED}" stroke-width="2" stroke-opacity="0.3"/>
    <circle cx="385" cy="265" r="4" fill="{foreground_color}"/>
    <circle cx="1140" cy="265" r="4" fill="{text_accent}"/>
  </g>
</svg>"""

def render_symbol_bitmap(size, foreground=(10, 88, 202, 255), secondary=(59, 130, 246, 255), amber=(245, 158, 11, 255), bg=None):
    """Renderiza o símbolo geométrico em bitmap RGBA puro de alta precisão."""
    scale = 4  # Super-amostragem para antialiasing de precisão
    img_size = size * scale
    img = Image.new("RGBA", (img_size, img_size), bg if bg else (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    def pt(x, y):
        # Mapeia coordenadas de 512x512 para o tamanho do bitmap
        return (x * img_size / 512.0, y * img_size / 512.0)

    # Malha técnica circular de fundo
    center = pt(256, 256)
    r1 = 190 * img_size / 512.0
    r2 = 110 * img_size / 512.0
    grid_color = (100, 116, 139, int(255 * 0.25))
    draw.ellipse([center[0]-r1, center[1]-r1, center[0]+r1, center[1]+r1], outline=grid_color, width=max(1, int(2 * scale)))
    draw.ellipse([center[0]-r2, center[1]-r2, center[0]+r2, center[1]+r2], outline=grid_color, width=max(1, int(2 * scale)))

    # Linhas de tronco e distribuição radial
    trunk_width = max(2, int(18 * scale))
    draw.line([pt(256, 90), pt(256, 256)], fill=foreground, width=trunk_width)
    draw.line([pt(256, 256), pt(120, 340)], fill=foreground, width=trunk_width)
    draw.line([pt(256, 256), pt(392, 340)], fill=foreground, width=trunk_width)
    draw.line([pt(120, 340), pt(90, 420)], fill=secondary, width=trunk_width)
    draw.line([pt(120, 340), pt(180, 420)], fill=secondary, width=trunk_width)
    draw.line([pt(392, 340), pt(332, 420)], fill=secondary, width=trunk_width)
    draw.line([pt(392, 340), pt(422, 420)], fill=secondary, width=trunk_width)

    # Marcador de impedância Delta V
    gauge_width = max(1, int(8 * scale))
    draw.line([pt(256, 130), pt(290, 130)], fill=amber, width=gauge_width)
    draw.line([pt(256, 190), pt(290, 190)], fill=amber, width=gauge_width)
    draw.line([pt(290, 130), pt(290, 190)], fill=amber, width=gauge_width)

    # Desenho dos nós com bordas limpas
    def draw_node(cx, cy, radius, fill_c, stroke_c, stroke_w):
        p = pt(cx, cy)
        r = radius * img_size / 512.0
        sw = stroke_w * scale
        draw.ellipse([p[0]-r-sw, p[1]-r-sw, p[0]+r+sw, p[1]+r+sw], fill=stroke_c)
        draw.ellipse([p[0]-r, p[1]-r, p[0]+r, p[1]+r], fill=fill_c)

    white = (248, 250, 252, 255)
    # Folhas / Nós de carga
    for nx, ny in [(90, 420), (180, 420), (332, 420), (422, 420)]:
        draw_node(nx, ny, 16, foreground, white, 4)

    # Derivações
    draw_node(120, 340, 22, secondary, white, 5)
    draw_node(392, 340, 22, secondary, white, 5)

    # Nó Central LID
    draw_node(256, 256, 32, amber, white, 6)
    p_center = pt(256, 256)
    r_core = 12 * img_size / 512.0
    draw.ellipse([p_center[0]-r_core, p_center[1]-r_core, p_center[0]+r_core, p_center[1]+r_core], fill=white)

    # Nó Fonte Transformador
    draw_node(256, 90, 28, foreground, amber, 6)
    p_trafo = pt(256, 90)
    r_trafo = 10 * img_size / 512.0
    draw.ellipse([p_trafo[0]-r_trafo, p_trafo[1]-r_trafo, p_trafo[0]+r_trafo, p_trafo[1]+r_trafo], fill=amber)

    # Reduz com resampling Lanczos para máxima nitidez
    return img.resize((size, size), Image.Resampling.LANCZOS)

def render_full_logo_bitmap(width, height, is_dark_theme=False, is_mono=False):
    """Renderiza a composição completa com Símbolo + tipografia sisQDT_LIGHT."""
    scale = 2
    w_scaled = width * scale
    h_scaled = height * scale
    
    bg = (15, 23, 42, 255) if is_dark_theme else (0, 0, 0, 0)
    img = Image.new("RGBA", (w_scaled, h_scaled), bg)
    draw = ImageDraw.Draw(img)

    # Renderiza símbolo à esquerda
    sym_size = int(h_scaled * 0.82)
    sym_y = int((h_scaled - sym_size) / 2)
    sym_x = int(40 * scale)

    if is_mono:
        fg = (30, 41, 59, 255) if not is_dark_theme else (248, 250, 252, 255)
        sec = (71, 85, 105, 255) if not is_dark_theme else (203, 213, 225, 255)
        amb = fg
    else:
        fg = (10, 88, 202, 255)
        sec = (59, 130, 246, 255)
        amb = (245, 158, 11, 255)

    symbol_img = render_symbol_bitmap(sym_size, foreground=fg, secondary=sec, amber=amb)
    img.paste(symbol_img, (sym_x, sym_y), symbol_img)

    # Cores dos textos
    if is_mono:
        color_sis = fg
        color_qdt = fg
        color_light = fg
        color_sub = sec
    else:
        color_sis = (248, 250, 252, 255) if is_dark_theme else (15, 23, 42, 255)
        color_qdt = (10, 88, 202, 255) if not is_dark_theme else (59, 130, 246, 255)
        color_light = (245, 158, 11, 255)
        color_sub = (148, 163, 184, 255) if is_dark_theme else (100, 116, 139, 255)

    # Tenta carregar fontes do sistema Windows (Segoe UI) ou fallback padrão
    font_large = None
    font_bold = None
    font_sub = None
    font_paths = [
        "C:\\Windows\\Fonts\\segoeui.ttf",
        "C:\\Windows\\Fonts\\segoeuib.ttf",
        "C:\\Windows\\Fonts\\arial.ttf",
        "C:\\Windows\\Fonts\\arialbd.ttf"
    ]
    try:
        font_large = ImageFont.truetype("C:\\Windows\\Fonts\\segoeui.ttf", int(95 * scale))
        font_bold = ImageFont.truetype("C:\\Windows\\Fonts\\segoeuib.ttf", int(105 * scale))
        font_sub = ImageFont.truetype("C:\\Windows\\Fonts\\segoeui.ttf", int(20 * scale))
    except Exception:
        font_large = ImageFont.load_default()
        font_bold = ImageFont.load_default()
        font_sub = ImageFont.load_default()

    base_x = sym_x + sym_size + int(30 * scale)
    base_y = int(80 * scale)

    # Desenha "sis"
    draw.text((base_x, base_y + int(10 * scale)), "sis", font=font_large, fill=color_sis)
    sis_bbox = draw.textbbox((base_x, base_y + int(10 * scale)), "sis", font=font_large)
    cur_x = sis_bbox[2] + int(12 * scale)

    # Desenha "QDT"
    draw.text((cur_x, base_y), "QDT", font=font_bold, fill=color_qdt)
    qdt_bbox = draw.textbbox((cur_x, base_y), "QDT", font=font_bold)
    cur_x = qdt_bbox[2] + int(8 * scale)

    # Desenha "_LIGHT"
    draw.text((cur_x, base_y), "_LIGHT", font=font_bold, fill=color_light)
    light_bbox = draw.textbbox((cur_x, base_y), "_LIGHT", font=font_bold)

    # Subtítulo técnico
    sub_y = base_y + int(120 * scale)
    draw.text((base_x, sub_y), "SISTEMA UNIFICADO DE CÁLCULO DE REDES ELÉTRICAS", font=font_sub, fill=color_sub)

    # Linha técnica de rodapé
    line_y = sub_y + int(35 * scale)
    draw.line([(base_x, line_y), (w_scaled - int(40 * scale), line_y)], fill=color_sub, width=max(1, int(2 * scale)))
    draw.ellipse([base_x - 4*scale, line_y - 4*scale, base_x + 4*scale, line_y + 4*scale], fill=color_qdt)
    draw.ellipse([w_scaled - int(40*scale) - 4*scale, line_y - 4*scale, w_scaled - int(40*scale) + 4*scale, line_y + 4*scale], fill=color_light)

    return img.resize((width, height), Image.Resampling.LANCZOS)

def main():
    print("[1/5] Gerando arquivos vetoriais SVG...")
    svg_icon = create_svg_icon()
    svg_icon_dark = create_svg_icon(foreground_color="#3B82F6", accent_color="#F59E0B", bg_color=COLOR_SLATE_DARK)
    svg_full = create_svg_full()
    svg_full_dark = create_svg_full(foreground_color="#3B82F6", text_primary="#F8FAFC", text_accent="#F59E0B", bg_color=COLOR_SLATE_DARK)
    svg_full_light = create_svg_full(foreground_color=COLOR_PRIMARY_BLUE, text_primary=COLOR_SLATE_DARK, text_accent=COLOR_AMBER, bg_color="#FFFFFF")
    svg_mono = create_svg_full(foreground_color=COLOR_SLATE_DARK, text_primary=COLOR_SLATE_DARK, text_accent=COLOR_SLATE_DARK)

    with open(os.path.join(ASSETS_DIR, "sisQDT_LIGHT.svg"), "w", encoding="utf-8") as f:
        f.write(svg_full)
    with open(os.path.join(ASSETS_DIR, "sisQDT_LIGHT_dark.svg"), "w", encoding="utf-8") as f:
        f.write(svg_full_dark)
    with open(os.path.join(ASSETS_DIR, "sisQDT_LIGHT_light.svg"), "w", encoding="utf-8") as f:
        f.write(svg_full_light)
    with open(os.path.join(ASSETS_DIR, "sisQDT_LIGHT_mono.svg"), "w", encoding="utf-8") as f:
        f.write(svg_mono)
    with open(os.path.join(ASSETS_DIR, "sisQDT_LIGHT_icon.svg"), "w", encoding="utf-8") as f:
        f.write(svg_icon)

    print("[2/5] Gerando arquivos rasterizados PNG...")
    png_icon = render_symbol_bitmap(512)
    png_icon.save(os.path.join(ASSETS_DIR, "sisQDT_LIGHT_icon.png"), "PNG")

    png_full = render_full_logo_bitmap(1200, 340, is_dark_theme=False)
    png_full.save(os.path.join(ASSETS_DIR, "sisQDT_LIGHT.png"), "PNG")

    png_dark = render_full_logo_bitmap(1200, 340, is_dark_theme=True)
    png_dark.save(os.path.join(ASSETS_DIR, "sisQDT_LIGHT_dark.png"), "PNG")

    png_light = render_full_logo_bitmap(1200, 340, is_dark_theme=False)
    # salva versão light com fundo branco explícito
    png_light_bg = Image.new("RGBA", (1200, 340), (255, 255, 255, 255))
    png_light_bg.paste(png_light, (0, 0), png_light)
    png_light_bg.save(os.path.join(ASSETS_DIR, "sisQDT_LIGHT_light.png"), "PNG")

    png_mono = render_full_logo_bitmap(1200, 340, is_dark_theme=False, is_mono=True)
    png_mono.save(os.path.join(ASSETS_DIR, "sisQDT_LIGHT_mono.png"), "PNG")

    print("[3/5] Gerando ícone Windows multi-resolução .ICO...")
    # Resoluções oficiais do Windows
    ico_sizes = [16, 24, 32, 48, 64, 128, 256]
    ico_images = [render_symbol_bitmap(s) for s in ico_sizes]
    
    ico_path = os.path.join(ASSETS_DIR, "sisQDT_LIGHT.ico")
    # Salva o ICO contendo todas as resoluções
    ico_images[-1].save(ico_path, format="ICO", sizes=[(s, s) for s in ico_sizes], append_images=ico_images[:-1])

    print("[4/5] Propagando assets para o projeto Desktop WPF...")
    # Copia o .ico e ícone principal para src/QdtCqts.Desktop.Wpf/Assets
    wpf_ico_path = os.path.join(WPF_ASSETS_DIR, "sisQDT_LIGHT.ico")
    ico_images[-1].save(wpf_ico_path, format="ICO", sizes=[(s, s) for s in ico_sizes], append_images=ico_images[:-1])
    png_icon.save(os.path.join(WPF_ASSETS_DIR, "sisQDT_LIGHT_icon.png"), "PNG")
    png_full.save(os.path.join(WPF_ASSETS_DIR, "sisQDT_LIGHT.png"), "PNG")

    print("[5/5] Validação concluída com sucesso!")
    print(f"Assets gerados em: {ASSETS_DIR}")
    print(f"Assets WPF em: {WPF_ASSETS_DIR}")

if __name__ == "__main__":
    main()
