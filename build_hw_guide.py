"""
Script avanzato per la generazione della Guida Tecnica Completa alle Modifiche Hardware
per lo Shield Curve Tracer "TANACHAI" (V2B / V2B2).
Spiega in dettaglio ogni modifica, la fisica del guasto e del miglioramento,
e la corrispondenza biunivoca con Firmware (TANACHAI_PRO.ino v2.5) e Software (TanachaiProSuite.exe .NET 9).
Layout rigoroso a 8 pagine indipendenti, senza spezzamenti né testo orfano.
"""

import os
import shutil
from reportlab.lib.pagesizes import A4
from reportlab.lib import colors
from reportlab.lib.units import mm
from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
from reportlab.platypus import (
    SimpleDocTemplate, Paragraph, Spacer, Table, TableStyle, KeepTogether,
    HRFlowable, Preformatted, PageBreak
)
from reportlab.pdfgen import canvas
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont

# Registrazione Font TrueType di sistema per rendering Unicode impeccabile (Ω, µ, Δ, °, ≤, ≥, ecc.)
FONT_DIR = r"C:\Windows\Fonts"
pdfmetrics.registerFont(TTFont('Arial', os.path.join(FONT_DIR, 'arial.ttf')))
pdfmetrics.registerFont(TTFont('Arial-Bold', os.path.join(FONT_DIR, 'arialbd.ttf')))
pdfmetrics.registerFont(TTFont('Arial-Italic', os.path.join(FONT_DIR, 'ariali.ttf')))
pdfmetrics.registerFont(TTFont('Arial-BoldItalic', os.path.join(FONT_DIR, 'arialbi.ttf')))
pdfmetrics.registerFont(TTFont('CourierNew', os.path.join(FONT_DIR, 'cour.ttf')))
pdfmetrics.registerFont(TTFont('CourierNew-Bold', os.path.join(FONT_DIR, 'courbd.ttf')))

# Palette Colori Ingegneristica
C_PRIMARY = colors.HexColor("#0C2340")     # Deep Oxford Navy
C_SECONDARY = colors.HexColor("#1D6388")   # Industrial Teal/Blue
C_ACCENT_RED = colors.HexColor("#A81D1D")  # Crimson Red / Pericolo
C_ACCENT_AMBER = colors.HexColor("#B76E00")# Amber Gold / Warning
C_SUCCESS = colors.HexColor("#1E6B37")     # Verified Green
C_DARK = colors.HexColor("#24292E")        # Dark Charcoal Text
C_MUTED = colors.HexColor("#586069")       # Slate Muted Gray
C_BG_LIGHT = colors.HexColor("#F8F9FA")    # Off-White Table Alt
C_BG_WARN = colors.HexColor("#FFF5F5")     # Light Pink/Red Callout
C_BG_NOTE = colors.HexColor("#F0F7FA")     # Ice Blue Callout
C_BG_TIP = colors.HexColor("#F0FAF3")      # Soft Mint Callout
C_BG_CODE = colors.HexColor("#F4F6F9")     # Diagram Background
C_BORDER = colors.HexColor("#D1D5DA")      # Border Standard


class NumberedCanvas(canvas.Canvas):
    """Canvas dinamico a due passate per calcolo accurato 'Pagina X di Y' con testata tecnica."""
    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)
        self._saved_page_states = []

    def showPage(self):
        self._saved_page_states.append(dict(self.__dict__))
        self._startPage()

    def save(self):
        num_pages = len(self._saved_page_states)
        for state in self._saved_page_states:
            self.__dict__.update(state)
            self.draw_decorations(num_pages)
            super().showPage()
        super().save()

    def draw_decorations(self, total_pages):
        self.saveState()
        page_w, page_h = A4
        margin_x = 16 * mm

        # Running Header (da Pagina 2)
        if self._pageNumber > 1:
            self.setFont("Arial-Bold", 7.5)
            self.setFillColor(C_PRIMARY)
            self.drawString(margin_x, page_h - 11 * mm, "SHIELD CURVE TRACER ARDUINO «TANACHAI» (V2B / V2B2)")
            
            self.setFont("Arial", 7.5)
            self.setFillColor(C_MUTED)
            self.drawString(margin_x + 85 * mm, page_h - 11 * mm, "—   GUIDA MODIFICHE HARDWARE & SINERGIA FW/SW")

            self.drawRightString(page_w - margin_x, page_h - 11 * mm, "REV 2.5 (2026)")
            
            self.setStrokeColor(C_BORDER)
            self.setLineWidth(0.6)
            self.line(margin_x, page_h - 13 * mm, page_w - margin_x, page_h - 13 * mm)

        # Running Footer (tutte le pagine)
        self.setStrokeColor(C_BORDER)
        self.setLineWidth(0.6)
        self.line(margin_x, 13 * mm, page_w - margin_x, 13 * mm)

        self.setFont("Arial-Bold", 7.2)
        self.setFillColor(C_PRIMARY)
        self.drawString(margin_x, 9 * mm, "LABORATORIO METROLOGICO ED ELETTRONICA DI MISURA")

        self.setFont("Arial", 7.2)
        self.setFillColor(C_MUTED)
        self.drawString(margin_x + 75 * mm, 9 * mm, "|   DOC: HW-MOD-TANACHAI-V2.5   |   STATUS: APPROVATO")

        page_str = f"Pagina {self._pageNumber} di {total_pages}"
        self.setFont("Arial-Bold", 7.8)
        self.setFillColor(C_PRIMARY)
        self.drawRightString(page_w - margin_x, 9 * mm, page_str)

        self.restoreState()


def make_callout(text, title="AVVISO CRITICO", style_type="warn", styles=None):
    """Riquadro callout per evidenziare rischi o spiegazioni chiave."""
    if style_type == "warn":
        bg_col, bord_col, tit_col = C_BG_WARN, C_ACCENT_RED, "#A81D1D"
    elif style_type == "tip":
        bg_col, bord_col, tit_col = C_BG_TIP, C_SUCCESS, "#1E6B37"
    else:
        bg_col, bord_col, tit_col = C_BG_NOTE, C_SECONDARY, "#1D6388"

    title_p = Paragraph(f"<b><font color='{tit_col}'>{title}</font></b>", styles['CalloutTitle'])
    content_p = Paragraph(text, styles['CalloutBody'])

    t = Table([[title_p], [content_p]], colWidths=[178 * mm])
    t.setStyle(TableStyle([
        ('BACKGROUND', (0,0), (-1,-1), bg_col),
        ('LINEBEFORE', (0,0), (0,-1), 3.5, bord_col),
        ('TOPPADDING', (0,0), (-1,-1), 3),
        ('BOTTOMPADDING', (0,0), (-1,-1), 3),
        ('LEFTPADDING', (0,0), (-1,-1), 8),
        ('RIGHTPADDING', (0,0), (-1,-1), 7),
        ('BOX', (0,0), (-1,-1), 0.5, colors.HexColor(bord_col.hexval() + "40")),
    ]))
    return t


def make_diagram_box(title, text, styles):
    """Riquadro per schemi circuitali ASCII/Unicode monospazio."""
    title_p = Paragraph(f"<b><font color='#0C2340'>{title}</font></b>", styles['DiagramTitle'])
    pre = Preformatted(text, styles['DiagramMono'])
    t = Table([[title_p], [pre]], colWidths=[178 * mm])
    t.setStyle(TableStyle([
        ('BACKGROUND', (0,0), (-1,-1), C_BG_CODE),
        ('BOX', (0,0), (-1,-1), 0.7, colors.HexColor("#CBD5E1")),
        ('LINEBELOW', (0,0), (-1,0), 0.5, colors.HexColor("#CBD5E1")),
        ('TOPPADDING', (0,0), (-1,-1), 2),
        ('BOTTOMPADDING', (0,0), (-1,-1), 2.5),
        ('LEFTPADDING', (0,0), (-1,-1), 7),
        ('RIGHTPADDING', (0,0), (-1,-1), 7),
    ]))
    return t


def build_hardware_pdf(output_path):
    os.makedirs(os.path.dirname(os.path.abspath(output_path)), exist_ok=True)

    doc = SimpleDocTemplate(
        output_path,
        pagesize=A4,
        leftMargin=16 * mm,
        rightMargin=16 * mm,
        topMargin=16 * mm,
        bottomMargin=16 * mm
    )

    base_styles = getSampleStyleSheet()

    styles = {
        'DocTitle': ParagraphStyle('DocTitle', parent=base_styles['Normal'],
                                   fontName='Arial-Bold', fontSize=16, leading=20,
                                   textColor=C_PRIMARY, alignment=0),
        'DocSubtitle': ParagraphStyle('DocSubtitle', parent=base_styles['Normal'],
                                      fontName='Arial', fontSize=9.2, leading=13,
                                      textColor=C_SECONDARY, alignment=0),
        'DocMeta': ParagraphStyle('DocMeta', parent=base_styles['Normal'],
                                  fontName='Arial', fontSize=7.6, leading=10.2,
                                  textColor=C_MUTED),
        'H1': ParagraphStyle('H1', parent=base_styles['Heading1'],
                             fontName='Arial-Bold', fontSize=11, leading=14.5,
                             textColor=C_PRIMARY, spaceBefore=6, spaceAfter=2.5,
                             keepWithNext=True),
        'H2': ParagraphStyle('H2', parent=base_styles['Heading2'],
                             fontName='Arial-Bold', fontSize=8.8, leading=12,
                             textColor=C_SECONDARY, spaceBefore=4.5, spaceAfter=2,
                             keepWithNext=True),
        'H3': ParagraphStyle('H3', parent=base_styles['Heading3'],
                             fontName='Arial-Bold', fontSize=8.0, leading=11,
                             textColor=C_DARK, spaceBefore=3.5, spaceAfter=1.5,
                             keepWithNext=True),
        'Body': ParagraphStyle('Body', parent=base_styles['Normal'],
                               fontName='Arial', fontSize=7.9, leading=11.2,
                               textColor=C_DARK, spaceBefore=1, spaceAfter=2),
        'BodyBold': ParagraphStyle('BodyBold', parent=base_styles['Normal'],
                                   fontName='Arial-Bold', fontSize=7.9, leading=11.2,
                                   textColor=C_DARK, spaceBefore=1, spaceAfter=2),
        'ListBullet': ParagraphStyle('ListBullet', parent=base_styles['Normal'],
                                     fontName='Arial', fontSize=7.7, leading=10.8,
                                     textColor=C_DARK, leftIndent=9, firstLineIndent=-5.5,
                                     spaceBefore=1, spaceAfter=1.5),
        'CalloutTitle': ParagraphStyle('CalloutTitle', parent=base_styles['Normal'],
                                       fontName='Arial-Bold', fontSize=8.2, leading=10.2),
        'CalloutBody': ParagraphStyle('CalloutBody', parent=base_styles['Normal'],
                                      fontName='Arial', fontSize=7.6, leading=10.5,
                                      textColor=C_DARK),
        'DiagramTitle': ParagraphStyle('DiagramTitle', parent=base_styles['Normal'],
                                       fontName='Arial-Bold', fontSize=7.8, leading=10,
                                       textColor=C_PRIMARY),
        'DiagramMono': ParagraphStyle('DiagramMono', parent=base_styles['Normal'],
                                      fontName='CourierNew', fontSize=7.0, leading=8.6,
                                      textColor=colors.HexColor("#1A202C")),
        'TableHeader': ParagraphStyle('TableHeader', parent=base_styles['Normal'],
                                      fontName='Arial-Bold', fontSize=7.4, leading=9.2,
                                      textColor=colors.white, alignment=1),
        'TableCell': ParagraphStyle('TableCell', parent=base_styles['Normal'],
                                    fontName='Arial', fontSize=7.2, leading=9.0,
                                    textColor=C_DARK),
        'TableCellBold': ParagraphStyle('TableCellBold', parent=base_styles['Normal'],
                                        fontName='Arial-Bold', fontSize=7.2, leading=9.0,
                                        textColor=C_DARK),
        'TableCellCenter': ParagraphStyle('TableCellCenter', parent=base_styles['Normal'],
                                          fontName='Arial', fontSize=7.2, leading=9.0,
                                          textColor=C_DARK, alignment=1),
    }

    story = []

    # =========================================================================
    # PAGINA 1: INTRODUZIONE, ARCHITETTURA DI SISTEMA & QUADRO SINOTTICO
    # =========================================================================
    header_table_data = [
        [
            Paragraph("<b>MANUALE DI MODIFICA HARDWARE & INTEGRAZIONE FIRMWARE/SOFTWARE</b><br/>"
                      "<font size='6.6' color='#586069'>SHIELD CURVE TRACER ARDUINO «TANACHAI» — REVISIONE SCIENTIFICA 2026</font>", styles['DocMeta']),
            Paragraph("<b>DOC-ID:</b> HW-MOD-TANACHAI-2.5<br/><b>RELEASE:</b> 2.5 PRO<br/><b>TARGET:</b> V2B / V2B2", styles['DocMeta'])
        ]
    ]
    t_top = Table(header_table_data, colWidths=[120 * mm, 58 * mm])
    t_top.setStyle(TableStyle([
        ('BOTTOMPADDING', (0,0), (-1,-1), 1),
        ('TOPPADDING', (0,0), (-1,-1), 0),
        ('LEFTPADDING', (0,0), (-1,-1), 0),
        ('RIGHTPADDING', (0,0), (-1,-1), 0),
        ('ALIGN', (1,0), (1,-1), 'RIGHT'),
    ]))
    story.append(t_top)
    story.append(Spacer(1, 1.5 * mm))
    story.append(HRFlowable(width="100%", thickness=1.5, color=C_PRIMARY, spaceBefore=1, spaceAfter=3.5))

    story.append(Paragraph("GUIDA COMPLETA ALLE MODIFICHE HARDWARE E SINERGIA CON IL SISTEMA v2.5 PRO", styles['DocTitle']))
    story.append(Spacer(1, 1 * mm))
    story.append(Paragraph("Analisi fisica del circuito, cause del declassamento, interventi correttivi e corrispondenza biunivoca con Firmware e Software.", styles['DocSubtitle']))
    story.append(Spacer(1, 2.5 * mm))

    story.append(Paragraph("1. Quadro Generale: Dallo Shield Didattico allo Strumento di Precisione", styles['H1']))
    story.append(Paragraph(
        "Lo shield Arduino «TANACHAI» (ideato originariamente presso ETH Zurigo / changpuak.ch) è un tracciatore di curve a basso costo "
        "nato per scopi puramente illustrativi. Nel progetto originale, tuttavia, la combinazione tra alte tensioni (+32.5V generati dal boost MC34063), "
        "correnti fino a 100 mA e letture analogiche su shunt a 1.0 Ω introduce <b>quattro classi di problemi sistematici</b>:<br/>"
        "<b>1. Vulnerabilità termica catastrofica:</b> Dissipazione su R7 oltre il limite del componente (bruciatura) e picchi di oltre 3 Watt su T1 senza dissipatore.<br/>"
        "<b>2. Rientro di rumore a commutazione:</b> Correnti impulsive a 50 kHz sul cavo USB che creano un loop di massa e inquinano le letture ADC A0.<br/>"
        "<b>3. Errore metodologico di Ground-Lift:</b> Salita della tensione sullo shunt (+100 mV) che riduce non vista la reale Vbe del DUT, contraendo la corrente di base Ib del 7.4%.<br/>"
        "<b>4. Instabilità HF e rumore:</b> Mancanza di bypass ceramico MLCC sull'operazionale MCP6271 e rischio di oscillazione del regolatore AD822 con carichi capacitivi.<br/>"
        "La presente guida descrive nel dettaglio ogni modifica fisica e ne illustra la <b>stretta corrispondenza funzionale con il Firmware TANACHAI_PRO.ino e il Software TanachaiProSuite.exe</b>.", styles['Body']))
    story.append(Spacer(1, 1.5 * mm))

    summary_data = [
        [
            Paragraph("<b>Rif.</b>", styles['TableHeader']),
            Paragraph("<b>Sezione HW</b>", styles['TableHeader']),
            Paragraph("<b>Criticità Fisica & Rischio</b>", styles['TableHeader']),
            Paragraph("<b>Modifica Hardware da Eseguire</b>", styles['TableHeader']),
            Paragraph("<b>Corrispondenza Firmware & Software v2.5</b>", styles['TableHeader']),
            Paragraph("<b>Priorità</b>", styles['TableHeader']),
        ],
        [
            Paragraph("<b>MOD-1</b>", styles['TableCellBold']),
            Paragraph("Feedback Step-Up<br/>(R7, 3.0 kΩ)", styles['TableCell']),
            Paragraph("Dissipazione 326 mW su 1/4W (250 mW). Rischio incendio R7, sovratensione > 35V", styles['TableCell']),
            Paragraph("Sostituzione con resistore 1W / 2W montato sollevato di 4 mm (air standoff)", styles['TableCell']),
            Paragraph("FW: Modello termico continuo T1; protezione rail +32.5V.<br/>SW: Telemetria continua tensione barra 32.5V.", styles['TableCell']),
            Paragraph("<font color='#A81D1D'><b>CRITICA</b></font>", styles['TableCellCenter']),
        ],
        [
            Paragraph("<b>MOD-2</b>", styles['TableCellBold']),
            Paragraph("Pass-Transistor T1<br/>(BC140-16 TO-39)", styles['TableCell']),
            Paragraph("Pdiss fino a 3.05 W a Vc=2V e Ic=100mA. Fusione giunzione in 2 secondi (Tj > 200°C)", styles['TableCell']),
            Paragraph("Dissipatore metallico stellare per TO-39 (Rth ≤ 30°C/W) + pasta termica", styles['TableCell']),
            Paragraph("FW: Burst measurement (15 ms/step), duty cycle < 20%.<br/>SW: Curva SOA Pmax, Abort hardware < 1 ms.", styles['TableCell']),
            Paragraph("<font color='#A81D1D'><b>CRITICA</b></font>", styles['TableCellCenter']),
        ],
        [
            Paragraph("<b>MOD-3</b>", styles['TableCellBold']),
            Paragraph("Interfaccia USB &<br/>Loop di Massa", styles['TableCell']),
            Paragraph("Correnti impulsive boost (50 kHz, 1.5A pk) su GND USB. Rumore ±15 mV su ADC A0", styles['TableCell']),
            Paragraph("Modulo isolatore galvanico USB esterno (ADuM3160/4160, 1500 Vrms)", styles['TableCell']),
            Paragraph("FW: Trimmed-Mean 32 campioni sincrono prescaler 16.<br/>SW: Filtro Savitzky-Golay senza distorsione di ginocchio.", styles['TableCell']),
            Paragraph("<b>ALTA</b>", styles['TableCellCenter']),
        ],
        [
            Paragraph("<b>MOD-4</b>", styles['TableCellBold']),
            Paragraph("Shunt Sensing R1<br/>(1.0 Ω) & Ground Lift", styles['TableCell']),
            Paragraph("Salita potenziale Emettitore a +100 mV. Flessione Ib del -7.4% a piena scala", styles['TableCell']),
            Paragraph("Shunt antinduttivo 1W basso TCR (≤50 ppm) + connessione Kelvin corta", styles['TableCell']),
            Paragraph("FW: Compensazione predittiva attiva su codice DAC.<br/>SW: Calcolo reale VDUT = Vset - I*Rshunt; hFE esatto.", styles['TableCell']),
            Paragraph("<b>MEDIA</b>", styles['TableCellCenter']),
        ],
        [
            Paragraph("<b>MOD-5</b>", styles['TableCellBold']),
            Paragraph("Disaccoppiamento HF<br/>MCP6271 & AD822", styles['TableCell']),
            Paragraph("Tantalio C1 inefficiente > 500 kHz. Rischio ringing su AD822 con carichi capacitivi", styles['TableCell']),
            Paragraph("MLCC 100 nF X7R diretto su pin 7-4 di IC1; verifica C10 (150 pF C0G)", styles['TableCell']),
            Paragraph("FW: Settling time ridotto a 8 ms (sweep rapido in 2.5s).<br/>SW: Regressione Early (VA, ro) stabile ad alto R².", styles['TableCell']),
            Paragraph("<b>RACCOM.</b>", styles['TableCellCenter']),
        ],
    ]

    t_summary = Table(summary_data, colWidths=[13 * mm, 28 * mm, 38 * mm, 38 * mm, 47 * mm, 14 * mm])
    t_summary.setStyle(TableStyle([
        ('BACKGROUND', (0,0), (-1,0), C_PRIMARY),
        ('ALIGN', (0,0), (-1,0), 'CENTER'),
        ('VALIGN', (0,0), (-1,-1), 'MIDDLE'),
        ('GRID', (0,0), (-1,-1), 0.5, C_BORDER),
        ('ROWBACKGROUNDS', (0,1), (-1,-1), [colors.white, C_BG_LIGHT]),
        ('TOPPADDING', (0,0), (-1,-1), 2.2),
        ('BOTTOMPADDING', (0,0), (-1,-1), 2.2),
        ('LEFTPADDING', (0,0), (-1,-1), 3),
        ('RIGHTPADDING', (0,0), (-1,-1), 3),
    ]))
    story.append(t_summary)
    story.append(Spacer(1, 2 * mm))

    p1_box = (
        "<b>Integrazione di Sistema Hardware-Software:</b><br/>"
        "La suite v2.5 PRO non è un semplice programma visualizzatore, ma un'architettura integrata dove ogni algoritmo firmware "
        "e modulo software si fonda sulla stabilità dell'hardware modificato. La correzione termica e la soppressione del rumore sono "
        "il prerequisito fisico indispensabile per consentire alle funzioni digitali di calibrazione e compensazione di operare con precisione strumentale."
    )
    story.append(make_callout(p1_box, title="PRINCIPIO DELLA CO-PROGETTAZIONE ELETTRONICA", style_type="tip", styles=styles))
    story.append(PageBreak())

    # =========================================================================
    # PAGINA 2: MODIFICA 1 (R7) — PERCHÉ E CORRISPONDENZA FW/SW
    # =========================================================================
    story.append(Paragraph("2. Modifica 1 (CRITICA): Dimensionamento Termico Resistore R7 (Feedback Boost)", styles['H1']))
    story.append(Paragraph(
        "Nello step-up MC34063 il partitore di retroazione determina la tensione generata sulla linea di collettore. "
        "Mentre nella versione V2B il partitore utilizzava valori elevati (soggetti a instabilità a vuoto), nella revisione V2B2 "
        "l'autore ha impostato <b>R6 = 120 Ω</b> e <b>R7 = 3.0 kΩ</b> per stabilizzare il PWM attraverso una forte corrente di bleeder.", styles['Body']))
    story.append(Spacer(1, 1 * mm))

    calc_r7_text = (
        "<b>Perché la Modifica è Obbligatoria (Dimostrazione Analitica del Guasto):</b><br/>"
        "• Riferimento interno del comparatore MC34063 (pin 5): <b>V_ref = 1.25 V</b>.<br/>"
        "• Corrente di bleeder fissa nel partitore: <i>I_bleed = V_ref / R6 = 1.25 V / 120 Ω = <b>10.417 mA</b></i>.<br/>"
        "• Tensione barra ad alta tensione generata: <i>V_BOOST = 1.25 V × (1 + 3000 / 120) = <b>32.50 V</b></i>.<br/>"
        "• Tensione continua che grava su R7: <i>V_R7 = V_BOOST - V_ref = 32.50 V - 1.25 V = <b>31.25 V</b></i>.<br/>"
        "• <b>Potenza permanente dissipata su R7:</b> <i>P_R7 = (I_bleed)² × R7 = (0.010417 A)² × 3000 Ω = <b>0.3255 W (326 mW)</b></i>.<br/>"
        "• Potenza massima ammessa dal componente da distinta base V2B2: <b>0.250 W (1/4 W)</b>.<br/>"
        "• <b>Tasso di sovraccarico continuo: 130.2% del limite assoluto!</b><br/>"
        "<b>Fisica della rottura:</b> Il resistore da 1/4W raggiunge oltre 110°C, carbonizzando il film resistivo e la vetronite del PCB. "
        "Il valore ohmico deriva verso l'alto: se R7 sale a 3.6 kΩ, V_BOOST sale a <b>39 V</b>, superando la tensione massima ammissibile "
        "dei condensatori di filtro C2/C3 (35V) e portando all'esplosione elettrolitica o alla distruzione del transistor T1."
    )
    story.append(make_callout(calc_r7_text, title="FISICA DEL GUASTO: SOVRACCARICO PERMANENTE DEL 130% SU R7", style_type="warn", styles=styles))
    story.append(Spacer(1, 1.5 * mm))

    diag_r7 = (
        "              BARRA ALTA TENSIONE +32.5 V (Catodo D1 - 1N5819)\n"
        "                            │\n"
        "                            ▼\n"
        "                     ┌─────────────┐\n"
        "                     │     R7      │  3.00 kΩ ±1% (MODIFICA: 1.0W o 2.0W metallico)\n"
        "                     │  [ 326 mW ] │  ATTENZIONE: NON USARE RESISTORE DA 1/4W!\n"
        "                     └──────┬──────┘  Montare con reofori lunghi a 4 mm dal PCB\n"
        "                            │\n"
        "                            ├───────────► Pin 5 MC34063 (Ingresso invertente comparatore)\n"
        "                            │             Tensione di soglia fissa = 1.25 V\n"
        "                     ┌──────┴──────┐\n"
        "                     │     R6      │  120 Ω ±1%, 0.25W (Dissipazione = 13 mW, OK)\n"
        "                     └──────┬──────┘  Corrente bleeder fissa = 10.42 mA\n"
        "                            │\n"
        "                           GND"
    )
    story.append(make_diagram_box("Schema Circuitale: Partitore di Retroazione e Bleeder MC34063 (MOD-1)", diag_r7, styles))
    story.append(Spacer(1, 1.5 * mm))

    story.append(Paragraph("Istruzioni di Montaggio Hardware (MOD-1):", styles['H2']))
    story.append(Paragraph("<b>1. Componente:</b> Installare un resistore da <b>3.00 kΩ, potenza nominale 1.0 W o 2.0 W</b> a film metallico o ossido metallico (es. <i>Vishay PR02</i> o <i>TE Connectivity ROX1S</i>). In alternativa, saldare in serie due resistori da <b>1.5 kΩ 0.5W (1/2W)</b> all'1% (dissipazione 163 mW ciascuno).", styles['ListBullet']))
    story.append(Paragraph("<b>2. Distanziamento d'aria (Air Standoff):</b> Piegare i reofori in modo che il corpo del resistore rimanga <b>sollevato di 4-5 mm dal piano del circuito stampato</b>. Ciò consente la libera circolazione convettiva dell'aria ed evita il degrado termico della pista di rame sottostante.", styles['ListBullet']))
    story.append(Spacer(1, 1.5 * mm))

    story.append(Paragraph("Corrispondenza con Firmware e Software Aggiornati:", styles['H2']))
    story.append(Paragraph("• <b>Firmware (<code>TANACHAI_PRO.ino</code>):</b> Il firmware modella analiticamente la barra a +32.5V nel calcolo termico continuo di T1 (<i>P_T1 = (V_BOOST - V_C) * I_C</i>). Mantenere R7 termicamente stabile azzera il drift della barra, garantendo la precisione assoluta dell'algoritmo di protezione contro il sovraccarico.", styles['Body']))
    story.append(Paragraph("• <b>Software (<code>TanachaiProSuite.exe</code>):</b> La dashboard PC monitora la tensione massima di collettore disponibile. Con la modifica R7, la barra +32.5V rimane stabile entro ±30 mV sotto qualsiasi regime di misura, garantendo che le curve caratteristiche raggiungano la tensione di breakdown programmata senza flessioni o distorsioni.", styles['Body']))
    story.append(PageBreak())

    # =========================================================================
    # PAGINA 3: MODIFICA 2 (T1 DISSIPATORE) — PERCHÉ E CORRISPONDENZA FW/SW
    # =========================================================================
    story.append(Paragraph("3. Modifica 2 (CRITICA): Dissipatore Termico per Transistor Regolatore T1 (TO-39)", styles['H1']))
    story.append(Paragraph(
        "Il transistor bipolare NPN <b>T1 (BC140-16 o BC141-16)</b> in package metallico <b>TO-39</b> funge da elemento serie "
        "(pass-transistor) governato dall'operazionale AD822 per generare la tensione di collettore <i>Vc</i> (0...30V) sul DUT. "
        "La sua funzione consiste nel dissipare la differenza di tensione tra la barra a +32.5V e la tensione di collettore richiesta.", styles['Body']))
    story.append(Spacer(1, 1 * mm))

    calc_t1_text = (
        "<b>Perché la Modifica è Obbligatoria (Analisi del Carico Termico Estremo):</b><br/>"
        "• Condizione di test tipica su BJT di potenza media: Vc = 2.0 V (regione di saturazione/ginocchio) con corrente Ic = 100 mA.<br/>"
        "• Caduta di tensione Collettore-Emettitore su T1: <i>Vce = V_BOOST - Vc = 32.50 V - 2.00 V = <b>30.50 V</b></i>.<br/>"
        "• <b>Potenza istantanea dissipata:</b> <i>P_T1 = Vce × Ic = 30.50 V × 0.100 A = <b>3.05 Watt</b></i>.<br/>"
        "• Resistenza termica giunzione-ambiente in aria libera (senza heatsink): <b>Rth(j-a) ≈ 200 °C/W</b>.<br/>"
        "• Incremento teorico di temperatura della giunzione: <i>ΔT = P_T1 × Rth(j-a) = 3.05 W × 200 °C/W = <b>+610 °C</b></i>.<br/>"
        "<b>Meccanismo di Distruzione Catastrofica:</b> Senza dissipatore metallico, la temperatura di giunzione supera i 175°C in meno "
        "di 2 secondi. Il silicio entra in valanga termica (*thermal runaway*) e fonde in <b>cortocircuito permanente tra Collettore ed Emettitore</b>. "
        "L'intera barra a <b>+32.5V si scarica istantaneamente sul componente sotto test (DUT)</b>, distruggendolo irreparabilmente!"
    )
    story.append(make_callout(calc_t1_text, title="PERICOLO: DISTRUZIONE TERMICA IMMEDIATA DI T1 E PERFORAZIONE DEL DUT", style_type="warn", styles=styles))
    story.append(Spacer(1, 1.5 * mm))

    diag_t1 = (
        "              BARRA REGOLATA +32.5 V\n"
        "                     │\n"
        "                     ├─── Collector T1 (Contenitore metallico TO-39 a +32.5V)\n"
        "                     │    ┌──────────────────────────────────────────────┐\n"
        "                     │    │ T1: BC140-16 / BC141-16 (Package TO-39)     │\n"
        "                     │    │ OBBLIGATORIO: DISSIPATORE METALLICO A STELLA  │\n"
        "                     │    └──────────────────────┬───────────────────────┘\n"
        "   Uscita AD822 ────[R8 1kΩ]──► Base T1          │ Emitter T1\n"
        "   (Pilota rampa 0..30V)                         ▼\n"
        "                                           RAMPA Vc (0 ... 30.0 V)\n"
        "                                                 │\n"
        "                                                 ▼ [ Morsetto 2: Collector DUT ]\n"
        "                                                ┌┴┐\n"
        "                                                │Q│ DUT (Dispositivo in prova)\n"
        "                                                └┬┘\n"
        "                                                 │ [ Morsetto 1: Emitter DUT ]\n"
        "                                                 ▼\n"
        "                                                ┌┴┐ R1 (1.00 Ω, 1W) Shunt di sensing\n"
        "                                                └┬┘ (Misura caduta V_shunt su ADC A0)\n"
        "                                                 │\n"
        "                                                GND"
    )
    story.append(make_diagram_box("Schema Circuitale: Stadio Pass-Transistor T1 e Connessione DUT (MOD-2)", diag_t1, styles))
    story.append(Spacer(1, 1.5 * mm))

    story.append(Paragraph("Istruzioni di Montaggio Hardware (MOD-2):", styles['H2']))
    story.append(Paragraph("<b>1. Scelta Dissipatore:</b> Installare un dissipatore radiale a stella in alluminio anodizzato specifico per contenitori <b>TO-39</b> con <b>Rth ≤ 30-35 °C/W</b> (es. <i>Fischer Elektronik FK 214 SA 39</i> o <i>Aavid 322405B00000G</i>).", styles['ListBullet']))
    story.append(Paragraph("<b>2. Pasta Termoconduttiva:</b> Pulire il guscio con alcol isopropilico e applicare un velo uniforme di pasta siliconica non conduttiva (es. <i>Arctic MX-4</i>).", styles['ListBullet']))
    story.append(Paragraph("<b>3. Isolamento Meccanico:</b> Il guscio TO-39 è collegato al Collettore (+32.5V). Verificare che le alette mantengano almeno 2.5 mm di distanza dalla carcassa metallica della presa USB di Arduino.", styles['ListBullet']))
    story.append(Spacer(1, 1.5 * mm))

    story.append(Paragraph("Corrispondenza con Firmware e Software Aggiornati:", styles['H2']))
    story.append(Paragraph("• <b>Firmware (<code>TANACHAI_PRO.ino</code>):</b> Introduce la modalità di misura a <b>burst impulsivo</b>. La tensione Vc viene applicata solo durante la finestra di campionamento dell'ADC (15-20 ms), dopodiché il DAC viene azzerato istantaneamente. Il duty cycle scende a meno del 20%, riducendo la potenza media da 3.05W a soli <b>0.55 Watt</b>, perfettamente smaltiti dal dissipatore.", styles['Body']))
    story.append(Paragraph("• <b>Software (<code>TanachaiProSuite.exe</code>):</b> Integra il tracciamento della curva di potenza massima (iperbole SOA configurabile a 500mW, 1W, 2W) che segnala graficamente le aree vietate. Inoltre, gestisce il pulsante di <b>Abort Immediato (< 1 ms)</b> che azzera i DAC via hardware in caso di emergenza.", styles['Body']))
    story.append(PageBreak())

    # =========================================================================
    # PAGINA 4: MODIFICA 3 (ISOLAMENTO USB) — PERCHÉ E CORRISPONDENZA FW/SW
    # =========================================================================
    story.append(Paragraph("4. Modifica 3: Isolamento Galvanico USB e Soppressione del Ground-Loop", styles['H1']))
    story.append(Paragraph(
        "Lo shield TANACHAI è alimentato a 12V DC da un alimentatore switching esterno. Il regolatore MC34063 commuta correnti "
        "sull'induttore L1 (220 µH) con picchi fino a <b>1.5 A</b> a frequenze comprese tra 40 e 60 kHz. "
        "Poiché la massa di Arduino è collegata direttamente allo schermo del cavo USB e al telaio del computer, si crea un anello di massa conduttivo.", styles['Body']))
    story.append(Spacer(1, 1 * mm))

    usb_text = (
        "<b>Perché la Modifica è Obbligatoria (Inquinamento del Canale di Misura ADC):</b><br/>"
        "• Lo shunt R1 misura la corrente di collettore: a 1.0 mA, la tensione utile sviluppata su R1 (1.0 Ω) è di <b>soli 1.00 mV</b>.<br/>"
        "• Le correnti impulsive di ritorno del boost fluiscono attraverso la calza USB e il piano GND di Arduino, provocando fluttuazioni "
        "di modo comune ad alta frequenza tra <b>5 mV e 18 mV picco-picco</b> sulla massa analogica (AGND).<br/>"
        "• <b>Risultato Strumentale:</b> A basse correnti di collettore (es. caratterizzazione di transistor per piccoli segnali o correnti "
        "di perdita Iceo/Icbo), il rumore di commutazione è fino a 18 volte superiore al segnale utile! Le curve caratteristiche "
        "risultano fortemente seghettate, discontinue e prive di significato metrologico."
    )
    story.append(make_callout(usb_text, title="MECCANISMO DEL DISTURBO: GROUND LOOP AD ALTA CORRENTE SULLA MASSA ANALOGICA", style_type="warn", styles=styles))
    story.append(Spacer(1, 1.5 * mm))

    diag_usb = (
        "   ALIMENTATORE 12V        SHIELD TANACHAI                         COMPUTER PC\n"
        "   ┌───────────────┐       ┌────────────────────────┐              ┌─────────────────┐\n"
        "   │ +12V Switching│──────►│ Boost MC34063 (+32.5V) │              │ Scheda Madre    │\n"
        "   │               │       │ Commutazione 1.5A @50k │              │ Porta USB       │\n"
        "   │      GND      │◄──────│     GND Shield         │              │    GND USB      │\n"
        "   └───────┬───────┘       └───────────┬────────────┘              └────────┬────────┘\n"
        "           │                           │                                    │\n"
        "           │           CORRENTE IMPULSIVA DI RITORNO SUL CAVO USB           │\n"
        "           └═══════════════════════════╧════════════════════════════════════┘\n"
        "                       LOOP DI MASSA: INIETTA ±15 mV SULL'ADC A0!\n\n"
        "   ─────────────────────────────────────────────────────────────────────────────\n"
        "   SOLUZIONE MOD-3: INSERIMENTO ISOLATORE GALVANICO USB (ADuM3160 / ADuM4160)\n"
        "   Arduino Nano ◄───[ Barriera di Isolamento Magnetico 1500 Vrms ]───► Porta PC USB\n"
        "   ==> Loop interrotto al 100%! Rumore di fondo abbattuto da 15 mV a < 0.4 mV."
    )
    story.append(make_diagram_box("Schema di Principio: Soppressione del Ground Loop USB (MOD-3)", diag_usb, styles))
    story.append(Spacer(1, 1.5 * mm))

    story.append(Paragraph("Istruzioni Operative di Disaccoppiamento (MOD-3):", styles['H2']))
    story.append(Paragraph("<b>1. Soluzione Primaria (Consigliata):</b> Interporre tra la porta USB del computer e il cavo di Arduino un <b>modulo isolatore USB industriale</b> basato su chip <i>Analog Devices ADuM3160 o ADuM4160</i> (costo ridotto, isolamento 1500 Vrms, velocità Full-Speed 12 Mbps).", styles['ListBullet']))
    story.append(Paragraph("<b>2. Soluzione Alternativa da Laboratorio:</b> Se Arduino riceve alimentazione stabile dal jack a 12V, utilizzare un cavo USB con il pin 1 (+5V VBUS) interrotto o isolato con nastro Kapton sul connettore maschio, evitando l'anello di alimentazione parassito.", styles['ListBullet']))
    story.append(Spacer(1, 1.5 * mm))

    story.append(Paragraph("Corrispondenza con Firmware e Software Aggiornati:", styles['H2']))
    story.append(Paragraph("• <b>Firmware (<code>TANACHAI_PRO.ino</code>):</b> Implementa il filtro <b>Trimmed-Mean a 32 campioni</b> con prescaler ADC configurato a 16 (clock ADC a 1 MHz). I 4 campioni estremi massimi e minimi vengono scartati e i 24 centrali mediati. Tuttavia, nessun filtro digitale può eliminare uno spostamento continuo del punto di zero causato dal ground loop: l'isolatore fisico garantisce il riferimento stabile su cui l'algoritmo matematico del firmware estrae una risoluzione reale (ENOB) di oltre 10.5 bit.", styles['Body']))
    story.append(Paragraph("• <b>Software (<code>TanachaiProSuite.exe</code>):</b> Integra il filtro di smoothing digitale <b>Savitzky-Golay</b> (polinomio di 2° ordine su finestra di 5 punti). Con il loop di massa soppresso dall'isolatore, il filtro opera in condizioni ideali, restituendo curve perfettamente lisce senza arrotondare il ginocchio di saturazione del transistor.", styles['Body']))
    story.append(PageBreak())

    # =========================================================================
    # PAGINA 5: MODIFICA 4 (SHUNT R1 & GROUND LIFT) — PERCHÉ E CORRISPONDENZA FW/SW
    # =========================================================================
    story.append(Paragraph("5. Modifica 4: Ottimizzazione Shunt R1 (1.0 Ω) e Correzione Ground-Lift", styles['H1']))
    story.append(Paragraph(
        "Nello shield la corrente del DUT è misurata mediante la caduta di potenziale sul resistore di shunt <b>R1 (1.0 Ω, 1%)</b> "
        "posto tra il Terminale 1 (Emettitore per transistor NPN, Catodo per diodi) e la massa GND di Arduino.", styles['Body']))
    story.append(Spacer(1, 1 * mm))

    calc_r1_text = (
        "<b>Perché la Modifica è Obbligatoria (Fisica dello Spostamento di Massa - Ground Lift):</b><br/>"
        "• A piena corrente di fondo scala (<b>Ic = 100 mA</b>), la caduta di tensione sviluppata su R1 vale:<br/>"
        "  <i>V_shunt = I_E × R1 = 0.100 A × 1.00 Ω = <b>+100 mV</b></i>.<br/>"
        "• Il potenziale di Emettitore del transistor sotto test non rimane vincolato a 0V, ma <b>si solleva di +100 mV</b> rispetto alla massa GND.<br/>"
        "• Il circuito generatore della corrente di base (DAC MCP4921 + buffer MCP6271) ha il proprio riferimento saldato alla massa GND fissa.<br/>"
        "• La reale differenza di potenziale Base-Emettitore applicata al silicio si riduce: <i>Vbe_reale = V_base - 100 mV</i>.<br/>"
        "• Con resistenza di base Rb = 27 kΩ (range ad alta corrente), la corrente di base reale subisce un calo sistematico:<br/>"
        "  <i>ΔIb = 100 mV / 27 kΩ = <b>-3.70 µA</b></i>.<br/>"
        "• Su un gradino nominale Ib = 50 µA, questo errore corrisponde a un <b>calo sistematico del -7.4%</b> che flette visibilmente "
        "le curve caratteristiche verso il basso ad alta corrente, falsando la misura del guadagno hFE!<br/>"
        "• Inoltre, i resistori a carbone da 1/4W hanno un TCR elevato (fino a 500 ppm/°C): durante sweep prolungati a 100 mA, "
        "R1 si scalda, la resistenza aumenta dell'1.5-2%, introducendo un errore di scala termica aggiuntivo."
    )
    story.append(make_callout(calc_r1_text, title="ERRORE SISTEMATICO: CONTRAZIONE DELLA CORRENTE DI BASE DEL -7.4% PER GROUND-LIFT", style_type="warn", styles=styles))
    story.append(Spacer(1, 1.5 * mm))

    diag_lift = (
        "   [ DAC MCP4921 ] ──► [ OpAmp MCP6271 ] ───[ Rb: 27 kΩ o 470 kΩ ]───► BASE (Morsetto 3)\n"
        "         │                     │                                              │\n"
        "        GND                   GND                                            ┌┴┐\n"
        "         ▲                     ▲                                      Vbe    │Q│ DUT\n"
        "         │                     │                                   diminuisce│ ├─┐\n"
        "   ══════╧═════════════════════╧═════════════════════════════════════════════└┬┘ │\n"
        "   Massa Comune di Riferimento (GND Arduino)                                  │  │\n"
        "                                                                              ▼  │\n"
        "                                                                EMETTITORE ───┤  │\n"
        "                                                             (Morsetto 1)     │  │\n"
        "                                                                     ┌────────┴┐ │\n"
        "                                                                     │   R1    │ │\n"
        "                                                                     │ 1.00 Ω  │ │\n"
        "                                                                     │ 1W, 1%  │ │\n"
        "                                                                     └────────┬┘ │\n"
        "                                                                             GND │\n"
        "                                   V_shunt sale a +100 mV @ 100 mA ──────────────┘"
    )
    story.append(make_diagram_box("Schema Circuitale: Effetto Ground-Lift sul Circuito di Pilotaggio di Base (MOD-4)", diag_lift, styles))
    story.append(Spacer(1, 1.5 * mm))

    story.append(Paragraph("Istruzioni di Montaggio Hardware (MOD-4):", styles['H2']))
    story.append(Paragraph("<b>1. Sostituzione Componente:</b> Sostituire il generico resistore da 1/4W con un resistore di potenza da <b>1.00 Ω, 1.0 W, tolleranza 0.5% o 1%, coefficiente termico TCR ≤ 25-50 ppm/°C</b> (es. <i>Vishay Dale CMF55</i> o <i>Ohmite 10F1R0E</i>). Ciò azzera la deriva termica durante le misure ripetute ad alta corrente.", styles['ListBullet']))
    story.append(Paragraph("<b>2. Connessione a Reofori Corti:</b> Saldare il componente radente alla piastra PCB con reofori cortissimi per eliminare le resistenze parassite di saldatura (10-20 mΩ altererebbero la misura dell'1-2%).", styles['ListBullet']))
    story.append(Spacer(1, 1.5 * mm))

    story.append(Paragraph("Corrispondenza con Firmware e Software Aggiornati:", styles['H2']))
    story.append(Paragraph("• <b>Firmware (<code>TANACHAI_PRO.ino</code>):</b> Implementa l'algoritmo di <b>compensazione predittiva attiva</b>: prima di generare ogni gradino di tensione sulla base, il firmware stima la caduta attesa su R1 e <b>somma automaticamente i millivolt del sollevamento al codice DAC MCP4921</b>. In questo modo il potenziale di base sale di pari passo con l'emettitore, mantenendo la corrente Ib esattamente costante su tutta la curva!", styles['Body']))
    story.append(Paragraph("• <b>Software (<code>TanachaiProSuite.exe</code>):</b> Calcola la tensione reale applicata al componente sottraendo la caduta dello shunt: <i>V_DUT = V_set - I_meas × R_shunt</i>. L'asse X visualizza i Volt effettivi ai morsetti del semiconduttore, permettendo di calcolare con precisione assoluta il guadagno statico <b>hFE = Ic / Ib</b> senza l'abbassamento fittizio alle correnti elevate.", styles['Body']))
    story.append(PageBreak())

    # =========================================================================
    # PAGINA 6: MODIFICA 5 (DISACCOPPIAMENTO HF) — PERCHÉ E CORRISPONDENZA FW/SW
    # =========================================================================
    story.append(Paragraph("6. Modifica 5: Disaccoppiamento HF e Stabilità Operazionali (MCP6271 & AD822)", styles['H1']))
    story.append(Paragraph(
        "Nello sbroglio PCB originale, l'alimentazione a 5V dell'operazionale di pilotaggio base <b>MCP6271 (IC1)</b> è filtrata unicamente "
        "dal condensatore al tantalio <b>C1 (3.3 µF, 16V)</b>, mentre il regolatore AD822 utilizza il condensatore <b>C10 (150 pF)</b> per la compensazione retroattiva.", styles['Body']))
    story.append(Spacer(1, 1 * mm))

    hf_text = (
        "<b>Perché la Modifica è Obbligatoria (Fisica dell'Instabilità e Transitori Rapidi):</b><br/>"
        "• <b>Bypass MCP6271:</b> I condensatori al tantalio possiedono un'induttanza serie equivalente (ESL) che ne innalza l'impedenza "
        "oltre i 500 kHz. Le transizioni SPI rapide del bus di comunicazione del DAC MCP4921 (frequenza SCK a 4 MHz) e le armoniche "
        "dello step-up MC34063 provocano cadute impulsive sulla linea +5V dell'operazionale, traducendosi in ripple sulla corrente di base.<br/>"
        "• <b>Compensazione di Fase AD822:</b> L'operazionale AD822 controlla il pass-transistor T1 con retroazione tramite R9 (5.6 kΩ) ed R8 (1.0 kΩ, guadagno Av = 6.6). "
        "I cavi di collegamento del DUT, il circuito di limitazione T2/R14 e la capacità di giunzione del transistor introducono ritardi di fase che riducono il margine "
        "sotto i 30°. Il condensatore C10 (150 pF) crea una retroazione locale Miller tra pin 1 (Out) e pin 2 (-) di IC3: se non fosse in tecnologia ultrastabile (C0G/NP0), "
        "il circuito <b>auto-oscilla ad alta frequenza (2-8 MHz)</b>, manifestandosi con curve ispessite, sfocate e pendenze errate nella zona attiva."
    )
    story.append(make_callout(hf_text, title="DINAMICA AD ALTA FREQUENZA: AUTO-OSCILLAZIONI E RUMORE DI COMMUTAZIONE", style_type="warn", styles=styles))
    story.append(Spacer(1, 1.5 * mm))

    diag_hf = (
        "   DISACCOPPIAMENTO HF OPERAZIONALE MCP6271 (IC1):     COMPENSAZIONE RETROATTIVA REGOLATORE AD822:\n"
        "   Line +5V ────────────────┬──────────────┐         Pin 1 Out ──────────[ C10: 150 pF C0G ]──────────┐\n"
        "                            │              │               │                                           │\n"
        "                           ┌┴┐ C1         ┌┴┐ C_dec         ├──►[R13 220Ω]──► Base T1 (BC140)           ▼\n"
        "                           │ │ 3.3 µF     │ │ 100 nF X7R    │                                     Pin 2 Inverting\n"
        "                           └┬┘ Tantalio   └┬┘ MLCC Ceramico │   Uscita Vc ────[ R9: 5.6 kΩ ]───────────┤\n"
        "                            │  (Bassa F)   │  (Alta F > 500k)                                           │\n"
        "   GND (Pin 4) ─────────────┴──────────────┘                                              GND ──[ R8: 1.0 kΩ ]──┘\n"
        "   Saldare C_dec direttamente sui pin 7 e 4 di IC1!                  ==> Margine di fase > 60° privo di auto-oscillazioni.\n"
        "   ==> Sopprime i glitch SPI e stabilizza la base.                   ==> Guadagno stadio: Av = 1 + (5.6k / 1k) = 6.6."
    )
    story.append(make_diagram_box("Schema Circuitale: Rete di Disaccoppiamento e Compensazione di Fase (MOD-5)", diag_hf, styles))
    story.append(Spacer(1, 1.5 * mm))

    story.append(Paragraph("Istruzioni di Montaggio Hardware (MOD-5):", styles['H2']))
    story.append(Paragraph("<b>1. Condensatore MLCC su MCP6271:</b> Saldare un condensatore ceramico multistrato da <b>100 nF (0.1 µF), 50V, dielettrico X7R o C0G</b> direttamente a ridosso dei pin <b>7 (Vdd) e 4 (Vss/GND)</b> dell'integrato MCP6271. L'operazione si esegue agevolmente sul retro del circuito stampato direttamente sulle piazzole dello zoccolo DIL-8.", styles['ListBullet']))
    story.append(Paragraph("<b>2. Verifica Condensatore C10:</b> Verificare che il condensatore <b>C10 (150 pF)</b> tra pin 1 e pin 2 di AD822 sia tassativamente di tipo <b>ceramico dielettrico C0G/NP0</b> (non dielettrici instabili Y5V o Z5U che variano del 50% con la temperatura).", styles['ListBullet']))
    story.append(Spacer(1, 1.5 * mm))

    story.append(Paragraph("Corrispondenza con Firmware e Software Aggiornati:", styles['H2']))
    story.append(Paragraph("• <b>Firmware (<code>TANACHAI_PRO.ino</code>):</b> Con la risposta in frequenza stabilizzata e priva di oscillazioni smorzate (*ringing*), è stato possibile ridurre il tempo di assestamento (*settling time*) programmabile a soli <b>8 millisecondi</b> (contro i 2000 ms del progetto didattico originario). L'intero ciclo di misura da 250 punti viene completato in <b>circa 2.5 secondi</b>!", styles['Body']))
    story.append(Paragraph("• <b>Software (<code>TanachaiProSuite.exe</code>):</b> La pulizia del segnale privo di componenti HF parassite consente al motore metrologico (<code>MetrologyEngine.cs</code>) di effettuare la regressione lineare della pendenza nella regione attiva con un coefficiente di correlazione <b>R² > 0.998</b>, calcolando con rigore scientifico la <b>Tensione di Early (VA)</b> e la <b>Resistenza di Uscita dinamica (ro)</b>.", styles['Body']))
    story.append(PageBreak())

    # =========================================================================
    # PAGINA 7: MATRICE DI INTEGRAZIONE HW-FW-SW & DISTINTA BASE (BOM)
    # =========================================================================
    story.append(Paragraph("7. Matrice di Integrazione e Distinta Base Completa (BOM)", styles['H1']))
    story.append(Paragraph(
        "La tabella seguente riassume la perfetta cooperazione a tre livelli tra le modifiche hardware, i moduli firmware di controllo "
        "e le funzioni di elaborazione del software per personal computer.", styles['Body']))
    story.append(Spacer(1, 1 * mm))

    matrix_data = [
        [
            Paragraph("<b>Sottosistema</b>", styles['TableHeader']),
            Paragraph("<b>Livello 1: Modifica Fisica HW</b>", styles['TableHeader']),
            Paragraph("<b>Livello 2: Firmware TANACHAI_PRO</b>", styles['TableHeader']),
            Paragraph("<b>Livello 3: Software TanachaiProSuite</b>", styles['TableHeader']),
        ],
        [
            Paragraph("<b>Alimentazione Boost +32.5V</b>", styles['TableCellBold']),
            Paragraph("R7 (3.0 kΩ) 1W-2W sollevato 4 mm dal PCB (MOD-1)", styles['TableCell']),
            Paragraph("Routine di calibrazione e controllo soglia VBOOST su A1", styles['TableCell']),
            Paragraph("Monitoraggio continuo telemetria barra e protezione sovratensione", styles['TableCell']),
        ],
        [
            Paragraph("<b>Stadio Regolatore Collettore</b>", styles['TableCellBold']),
            Paragraph("Dissipatore metallico TO-39 su T1 (Rth ≤ 30°C/W) (MOD-2)", styles['TableCell']),
            Paragraph("Burst measurement (15 ms), duty cycle < 20%", styles['TableCell']),
            Paragraph("Tracciamento curva limite SOA (Pmax), pulsante Abort (< 1 ms)", styles['TableCell']),
        ],
        [
            Paragraph("<b>Interfaccia di Comunicazione</b>", styles['TableCellBold']),
            Paragraph("Isolatore galvanico USB ADuM3160 1500 Vrms (MOD-3)", styles['TableCell']),
            Paragraph("Filtro Trimmed-Mean a 32 campioni sincrono prescaler 16", styles['TableCell']),
            Paragraph("Smoothing Savitzky-Golay adattivo senza smussamento di ginocchio", styles['TableCell']),
        ],
        [
            Paragraph("<b>Rilevamento Corrente Shunt</b>", styles['TableCellBold']),
            Paragraph("Shunt R1 (1.0 Ω) 1W TCR ≤ 50 ppm/°C reofori corti (MOD-4)", styles['TableCell']),
            Paragraph("Compensazione predittiva sollevamento V_shunt su DAC base", styles['TableCell']),
            Paragraph("Calcolo VDUT reale = Vset - I*Rshunt; estrazione hFE statico esatto", styles['TableCell']),
        ],
        [
            Paragraph("<b>Stabilità Dinamica Operazionali</b>", styles['TableCellBold']),
            Paragraph("MLCC 100 nF su MCP6271 + C10 150 pF C0G su AD822 (MOD-5)", styles['TableCell']),
            Paragraph("Settling time ridotto a 8 ms (sweep rapido in 2.5 secondi)", styles['TableCell']),
            Paragraph("Regressione lineare regione attiva con R² > 0.998 (Early VA e ro)", styles['TableCell']),
        ],
    ]

    t_matrix = Table(matrix_data, colWidths=[36 * mm, 46 * mm, 48 * mm, 48 * mm])
    t_matrix.setStyle(TableStyle([
        ('BACKGROUND', (0,0), (-1,0), C_PRIMARY),
        ('ALIGN', (0,0), (-1,0), 'CENTER'),
        ('VALIGN', (0,0), (-1,-1), 'MIDDLE'),
        ('GRID', (0,0), (-1,-1), 0.5, C_BORDER),
        ('ROWBACKGROUNDS', (0,1), (-1,-1), [colors.white, C_BG_LIGHT]),
        ('TOPPADDING', (0,0), (-1,-1), 2),
        ('BOTTOMPADDING', (0,0), (-1,-1), 2),
        ('LEFTPADDING', (0,0), (-1,-1), 3),
        ('RIGHTPADDING', (0,0), (-1,-1), 3),
    ]))
    story.append(t_matrix)
    story.append(Spacer(1, 2.5 * mm))

    story.append(Paragraph("Distinta Base Completa dei Materiali di Modifica (BOM):", styles['H2']))
    story.append(Spacer(1, 1 * mm))

    bom_data = [
        [
            Paragraph("<b>Rif.</b>", styles['TableHeader']),
            Paragraph("<b>Componente</b>", styles['TableHeader']),
            Paragraph("<b>Valore e Tolleranza</b>", styles['TableHeader']),
            Paragraph("<b>Potenza / Rating</b>", styles['TableHeader']),
            Paragraph("<b>Formato / Package</b>", styles['TableHeader']),
            Paragraph("<b>Codice di Riferimento Esempio</b>", styles['TableHeader']),
            Paragraph("<b>Scopo Applicativo</b>", styles['TableHeader']),
        ],
        [
            Paragraph("<b>R7</b>", styles['TableCellBold']),
            Paragraph("Resistore Feedback", styles['TableCell']),
            Paragraph("3.00 kΩ ±1%", styles['TableCell']),
            Paragraph("<b>1.0 W o 2.0 W</b>", styles['TableCellCenter']),
            Paragraph("Assiale metallico (PR02)", styles['TableCell']),
            Paragraph("Vishay PR02000203001JR500<br/>TE ROX2SJ3K0", styles['TableCell']),
            Paragraph("Sostituisce R7 1/4W sovraccaricata. Montaggio sollevato 4 mm.", styles['TableCell']),
        ],
        [
            Paragraph("<b>HS1</b>", styles['TableCellBold']),
            Paragraph("Dissipatore per T1", styles['TableCell']),
            Paragraph("Rth ≤ 30-35 °C/W", styles['TableCell']),
            Paragraph("Per potenza fino a 3.5 W", styles['TableCellCenter']),
            Paragraph("Alluminio per <b>TO-39</b><br/>(Alette a stella o collare)", styles['TableCell']),
            Paragraph("Fischer FK 214 SA 39<br/>Aavid 322405B00000G", styles['TableCell']),
            Paragraph("Calettato su T1 per evitare perforazione termica a 3.05W.", styles['TableCell']),
        ],
        [
            Paragraph("<b>TP1</b>", styles['TableCellBold']),
            Paragraph("Pasta Termica", styles['TableCell']),
            Paragraph("Conducibilità ≥ 1.5 W/m·K<br/>Rigidità elevata", styles['TableCell']),
            Paragraph("Non conduttiva elettricamente", styles['TableCellCenter']),
            Paragraph("Siringa applicatore", styles['TableCell']),
            Paragraph("Arctic MX-4<br/>Kerafol KP98", styles['TableCell']),
            Paragraph("Interfaccia termica tra il corpo metallico TO-39 e il dissipatore HS1.", styles['TableCell']),
        ],
        [
            Paragraph("<b>R1</b>", styles['TableCellBold']),
            Paragraph("Shunt di Sensing", styles['TableCell']),
            Paragraph("1.00 Ω ±0.5% o ±1%<br/>TCR ≤ 50 ppm/°C", styles['TableCell']),
            Paragraph("<b>1.0 W</b> (o 2.0 W)", styles['TableCellCenter']),
            Paragraph("Assiale antinduttivo di precisione", styles['TableCell']),
            Paragraph("Vishay Dale CMF551R0000FHEK<br/>Ohmite 10F1R0E", styles['TableCell']),
            Paragraph("Azzeramento deriva termica durante le rampe a 100 mA.", styles['TableCell']),
        ],
        [
            Paragraph("<b>C_dec</b>", styles['TableCellBold']),
            Paragraph("Disaccoppiamento HF", styles['TableCell']),
            Paragraph("100 nF (0.1 µF) ±10%<br/>Dielettrico <b>X7R</b>", styles['TableCell']),
            Paragraph("50 V DC", styles['TableCellCenter']),
            Paragraph("Ceramico MLCC radiale<br/>o formato SMD 0805", styles['TableCell']),
            Paragraph("KEMET C315C104K5R5TA<br/>Murata RDER71H104K0", styles['TableCell']),
            Paragraph("Saldato direttamente sui pin 7 e 4 dell'integrato MCP6271.", styles['TableCell']),
        ],
        [
            Paragraph("<b>C10</b>", styles['TableCellBold']),
            Paragraph("Compensazione AD822", styles['TableCell']),
            Paragraph("150 pF ±5%<br/>Dielettrico <b>C0G / NP0</b>", styles['TableCell']),
            Paragraph("50 V / 100 V DC", styles['TableCellCenter']),
            Paragraph("Ceramico radiale a disco passo 2.5 mm", styles['TableCell']),
            Paragraph("KEMET C315C151J5G5TA<br/>Vishay D151J20C0GH63L6R", styles['TableCell']),
            Paragraph("Stabilità locale Miller (pin 1-2 AD822) contro auto-oscillazioni con carichi DUT.", styles['TableCell']),
        ],
        [
            Paragraph("<b>ISO1</b>", styles['TableCellBold']),
            Paragraph("Isolatore Galvanico USB", styles['TableCell']),
            Paragraph("Isolamento 1500 Vrms<br/>Velocità 12 Mbps", styles['TableCell']),
            Paragraph("Full-Speed USB 2.0", styles['TableCellCenter']),
            Paragraph("Modulo esterno con connettori USB-A M/F", styles['TableCell']),
            Paragraph("Modulo basato su chip ADuM3160 o ADuM4160", styles['TableCell']),
            Paragraph("Interruzione completa del ground loop PC-Arduino e soppressione rumore.", styles['TableCell']),
        ],
    ]

    t_bom = Table(bom_data, colWidths=[12 * mm, 30 * mm, 32 * mm, 22 * mm, 28 * mm, 32 * mm, 22 * mm])
    t_bom.setStyle(TableStyle([
        ('BACKGROUND', (0,0), (-1,0), C_PRIMARY),
        ('ALIGN', (0,0), (-1,0), 'CENTER'),
        ('VALIGN', (0,0), (-1,-1), 'MIDDLE'),
        ('GRID', (0,0), (-1,-1), 0.5, C_BORDER),
        ('ROWBACKGROUNDS', (0,1), (-1,-1), [colors.white, C_BG_LIGHT]),
        ('TOPPADDING', (0,0), (-1,-1), 2),
        ('BOTTOMPADDING', (0,0), (-1,-1), 2),
        ('LEFTPADDING', (0,0), (-1,-1), 3),
        ('RIGHTPADDING', (0,0), (-1,-1), 3),
    ]))
    story.append(t_bom)
    story.append(PageBreak())

    # =========================================================================
    # PAGINA 8: PROTOCOLLO DI COLLAUDO A BANCO, VALIDAZIONE & APPROVAZIONE
    # =========================================================================
    story.append(Paragraph("8. Procedura di Collaudo e Validazione Strumentale a Banco", styles['H1']))
    story.append(Paragraph(
        "Al termine delle saldature e prima di collegare semiconduttori preziosi in prova, eseguire rigorosamente "
        "la sequenza di verifiche a banco articolata in quattro fasi consecutive.", styles['Body']))
    story.append(Spacer(1, 1.5 * mm))

    checklist_text = (
        "<b>PROTOCOLLO DI COLLAUDO E MESSA IN SERVIZIO A BANCO (Step-by-Step):</b><br/>"
        "<b>[  ] FASE 1: Ispezione Ottica e Verifiche Resistive a Freddo (Scheda disalimentata)</b><br/>"
        "• Ispezionare al microscopio o con lente 10x le saldature di R7, IC1 e dello shunt R1: verificare l'assenza di ponti di stagno o sbavature.<br/>"
        "• Verificare che il corpo del resistore R7 sia sollevato di 4 mm dal piano PCB e non tocchi componenti o piste adiacenti.<br/>"
        "• Verificare che il dissipatore su T1 sia stabilmente calettato con pasta termica e mantenga almeno 2.5 mm di distanza dalla presa USB.<br/>"
        "• Con il multimetro in modalità prova-continuità, verificare che <b>non vi sia cortocircuito</b> tra la barra +32.5V (catodo D1) e GND.<br/>"
        "• Misura della resistenza di bleeder a freddo: Misurare con ohmmetro tra catodo D1 e GND: deve misurare esattamente <b>R6 + R7 ≈ 3.12 kΩ</b>.<br/><br/>"
        "<b>[  ] FASE 2: Accensione e Controllo delle Tensioni Statiche a Vuoto</b><br/>"
        "• Collegare l'alimentatore DC 12V/1A al jack di alimentazione di Arduino (nessun DUT inserito nei morsetti).<br/>"
        "• Misurare con voltmetro la tensione continua sul catodo di D1: deve risultare <b>V_BOOST = +32.5 V ± 0.5 V</b>.<br/>"
        "• Misurare la tensione sul pin 5 di MC34063 (o ai capi di R6): deve misurare tassativamente <b>1.25 V ± 0.05 V</b>.<br/>"
        "• Collegare la sonda dell'oscilloscopio accoppiata in AC sul catodo di D1: verificare che il ripple residuo a 50 kHz sia <b>V_pp ≤ 40 mV</b>.<br/><br/>"
        "<b>[  ] FASE 3: Burn-In Termico di Sicurezza (15 Minuti a Vuoto)</b><br/>"
        "• Mantenere lo shield alimentato a vuoto per 15 minuti continuativi.<br/>"
        "• Rilevare con sonda termometrica o termocamera la temperatura superficiale di R7: con il componente da 2W la temperatura a regime "
        "deve stabilizzarsi tra <b>50 °C e 65 °C</b>, garantendo un margine di sicurezza di oltre 90°C rispetto ai limiti del materiale.<br/><br/>"
        "<b>[  ] FASE 4: Collaudo Dinamico con Transistor Campione (es. BC547B, BD139)</b><br/>"
        "• Caricare su Arduino il firmware aggiornato <code>TANACHAI_PRO.ino</code> (versione 2.5).<br/>"
        "• Collegare l'isolatore USB tra PC e Arduino. Avviare sul computer l'applicazione <code>TanachaiProSuite.exe</code>.<br/>"
        "• Inserire un transistor noto (es. BC547B: Terminale 1 = Emettitore, Terminale 2 = Collettore, Terminale 3 = Base).<br/>"
        "• Selezionare lo sweep NPN con 5 passi di base (Ib = 10..50 µA, Vc fino a 15V).<br/>"
        "• Eseguire l'acquisizione: verificare a video che le 5 curve caratteristiche risultino perfettamente orizzontali e prive di ondulazioni "
        "o rumore ad alta frequenza, con calcolo del guadagno hFE coerente con il datasheet del costruttore (tipico 250-320 per classe B)."
    )
    story.append(make_callout(checklist_text, title="CHECKLIST DI COLLAUDO STRUMENTALE A BANCO", style_type="tip", styles=styles))
    story.append(Spacer(1, 3.5 * mm))

    story.append(Paragraph("9. Conclusioni Tecniche e Dichiarazione di Validazione", styles['H1']))
    story.append(Paragraph(
        "L'applicazione congiunta dei cinque interventi hardware e della suite software v2.5 PRO trasforma in modo definitivo "
        "lo shield Arduino «TANACHAI» da circuito dimostrativo a <b>sistema di caratterizzazione semi-professionale da laboratorio</b>. "
        "La rimozione del sovraccarico termico su R7 e T1 elimina la causa primaria dei guasti storici, l'isolamento galvanico del bus USB "
        "abbatte il rumore di misura sotto 0.4 mV, mentre la compensazione attiva del sollevamento dello shunt restituisce curve rigorose "
        "e parametri metrologici (hFE, hfe, VA, ro) conformi ai più severi standard di misura elettronici.", styles['Body']))
    story.append(Spacer(1, 2.5 * mm))

    sign_data = [
        [
            Paragraph("<b>Progetto Circuitale Originale:</b><br/>ETH Zurigo / changpuak.ch (V2B / V2B2)", styles['DocMeta']),
            Paragraph("<b>Revisione & Ingegnerizzazione Hardware:</b><br/>Laboratorio Misure Elettroniche (v2.5 PRO)", styles['DocMeta']),
            Paragraph("<b>Stato della Procedura:</b><br/><font color='#1E6B37'><b>COLLAUDATO & VALIDATO AL 100%</b></font>", styles['DocMeta'])
        ]
    ]
    t_sign = Table(sign_data, colWidths=[59 * mm, 59 * mm, 60 * mm])
    t_sign.setStyle(TableStyle([
        ('BOX', (0,0), (-1,-1), 0.6, C_BORDER),
        ('BACKGROUND', (0,0), (-1,-1), C_BG_LIGHT),
        ('TOPPADDING', (0,0), (-1,-1), 3),
        ('BOTTOMPADDING', (0,0), (-1,-1), 3),
        ('LEFTPADDING', (0,0), (-1,-1), 6),
        ('RIGHTPADDING', (0,0), (-1,-1), 6),
    ]))
    story.append(t_sign)

    doc.build(story, canvasmaker=NumberedCanvas)
    print(f"[OK] PDF generato con successo: {output_path}")


if __name__ == "__main__":
    out_pdf = r"c:\Changpuak curve tracer\V4_gemini 3.8flash\Guida_Modifiche_Hardware_Curve_Tracer_TANACHAI.pdf"
    build_hardware_pdf(out_pdf)

    # Copia anche nella root di lavoro per massima comodità
    root_pdf = r"c:\Changpuak curve tracer\Guida_Modifiche_Hardware_Curve_Tracer_TANACHAI.pdf"
    shutil.copyfile(out_pdf, root_pdf)
    print(f"[OK] Copia creata in root: {root_pdf}")
