"""Generate Resident Evil & Silent Hill inspired music and sound effects.
All WAVs are mono, 22050Hz or 48000Hz, peak <= 0.445 (-7.0 dBFS) to pass all audio checks.
"""
from pathlib import Path
import numpy as np, wave, hashlib, io
from scipy.signal import butter, sosfilt

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'Assets/Ecos/Resources/Audio'
OUT.mkdir(parents=True, exist_ok=True)
rng = np.random.default_rng(19992001)

def write_wav(name, samples, sr=48000, loop=False):
    a = np.asarray(samples, dtype=float)
    a -= np.mean(a)
    if not loop:
        u = np.arange(len(a)) / sr
        fade_in = np.minimum(u / 0.008, 1.0)
        fade_out = np.minimum((len(a) / sr - u) / 0.08, 1.0)
        a *= fade_in * fade_out
    peak = max(np.max(np.abs(a)), 1e-9)
    a = a / peak * 0.442
    buf = io.BytesIO()
    with wave.open(buf, 'wb') as f:
        f.setnchannels(1)
        f.setsampwidth(2)
        f.setframerate(sr)
        f.writeframes(np.int16(a * 32767).tobytes())
    wav_path = OUT / f'{name}.wav'
    wav_path.write_bytes(buf.getvalue())
    meta_path = OUT / f'{name}.wav.meta'
    if not meta_path.exists():
        guid = hashlib.md5(f'ecos-resh-{name}'.encode()).hexdigest()
        meta_path.write_text(f'''fileFormatVersion: 2
guid: {guid}
AudioImporter:
  externalObjects: {{}}
  serializedVersion: 7
  defaultSettings:
    serializedVersion: 2
    loadType: 0
    sampleRateSetting: 0
    sampleRateOverride: {sr}
    compressionFormat: 0
    quality: 1
    conversionMode: 0
  forceToMono: 1
  normalize: 0
  preloadAudioData: 1
  loadInBackground: 0
  ambisonic: 0
  3D: 0
''')
    print(f'{name}.wav ({sr} Hz, {len(a)/sr:.2f}s, peak={np.max(np.abs(a)):.3f})')
    return a

sr_amb = 22050
sr48 = 48000

def re_piano_note(freq, dur, sr=22050, warmth=1.0):
    tt = np.arange(int(dur * sr)) / sr
    sig = np.zeros_like(tt)
    for det in [-0.0026, 0.0, 0.0029]:
        f = freq * (1.0 + det)
        for harm, amp, dec in [(1, 1.0, 1.35), (2, 0.44 * warmth, 2.1), (3, 0.19 * warmth, 3.0), (4, 0.08, 4.4), (5, 0.035, 5.8)]:
            sig += amp * np.sin(2 * np.pi * f * harm * tt) * np.exp(-tt * dec)
    env = np.minimum(tt / 0.014, 1.0) * np.minimum((dur - tt) / 0.16, 1.0)
    return sig * env

def sh_guitar_pluck(freq, dur, sr=22050):
    tt = np.arange(int(dur * sr)) / sr
    sig = np.zeros_like(tt)
    for det in [-0.0018, 0.0018]:
        f = freq * (1.0 + det)
        for k in range(1, 11):
            sig += (1.0 / (k ** 1.15)) * np.sin(2 * np.pi * f * k * tt + 0.15 * np.sin(2 * np.pi * 4.8 * tt)) * np.exp(-tt * (1.1 + k * 0.55))
    env = np.minimum(tt / 0.006, 1.0) * np.minimum((dur - tt) / 0.12, 1.0)
    return sig * env

# 1. Theme.wav: Main Menu & Story Theme (Silent Hill 2 Promise / RE Save Room hybrid, 32s @ 22050Hz)
t32 = np.arange(sr_amb * 32) / sr_amb
theme = np.zeros_like(t32)
arp_notes = [
    (0.0, 293.66, 3.5), (0.75, 440.00, 3.0), (1.5, 523.25, 2.8), (2.25, 698.46, 2.5), (3.0, 659.25, 2.5),
    (4.0, 233.08, 3.5), (4.75, 349.23, 3.0), (5.5, 440.00, 2.8), (6.25, 587.33, 2.5), (7.0, 523.25, 2.5),
    (8.0, 196.00, 3.5), (8.75, 293.66, 3.0), (9.5, 392.00, 2.8), (10.25, 466.16, 2.5), (11.0, 440.00, 2.5),
    (12.0, 220.00, 3.8), (12.75, 329.63, 3.2), (13.5, 440.00, 3.0), (14.25, 554.37, 2.5), (15.0, 523.25, 2.5),
]
for cycle in [0.0, 16.0]:
    for start_t, hz, dur in arp_notes:
        idx = int((cycle + start_t) * sr_amb)
        note_wav = re_piano_note(hz, dur, sr_amb)
        if cycle == 16.0:
            note_wav = 0.65 * note_wav + 0.45 * sh_guitar_pluck(hz * 0.5, dur, sr_amb)
        end_idx = min(len(theme), idx + len(note_wav))
        theme[idx:end_idx] += note_wav[:end_idx - idx] * 0.55
        for delay_s, d_gain in [(0.375, 0.24), (0.75, 0.11)]:
            d_idx = idx + int(delay_s * sr_amb)
            if d_idx < len(theme):
                d_end = min(len(theme), d_idx + len(note_wav))
                theme[d_idx:d_end] += note_wav[:d_end - d_idx] * d_gain

for f_pad, g_pad in [(73.42, 0.36), (110.0, 0.26), (146.83, 0.22), (174.61, 0.18), (220.0, 0.12)]:
    vibrato = 0.0032 * np.sin(2 * np.pi * 0.38 * t32)
    theme += g_pad * np.sin(2 * np.pi * f_pad * (1 + vibrato) * t32) * (0.65 + 0.35 * np.sin(2 * np.pi * t32 / 8.0))
env32 = np.minimum(t32 / 1.2, 1.0) * np.minimum((32.0 - t32) / 1.5, 1.0)
write_wav('Theme', theme * env32, sr=sr_amb, loop=True)

# 2. SaveRoomRE.wav: Classic Resident Evil Save Room ("Secure Place" / "Free From Fear" style, 32s @ 22050Hz)
save_room = np.zeros_like(t32)
re_save_melody = [
    (0.0, 293.66, 4.2), (1.0, 349.23, 3.6), (2.0, 440.00, 3.8), (3.0, 523.25, 3.2), (3.5, 493.88, 2.8),
    (4.5, 233.08, 4.0), (5.5, 349.23, 3.5), (6.5, 440.00, 3.5), (7.25, 392.00, 3.0),
    (8.5, 196.00, 4.2), (9.5, 293.66, 3.6), (10.5, 349.23, 3.6), (11.25, 329.63, 3.0),
    (12.5, 220.00, 4.4), (13.5, 329.63, 3.6), (14.5, 415.30, 3.2), (15.25, 440.00, 2.8),
]
for cycle in [0.0, 16.0]:
    for start_t, hz, dur in re_save_melody:
        idx = int((cycle + start_t) * sr_amb)
        n_wav = re_piano_note(hz, dur, sr_amb, warmth=0.8)
        if cycle == 16.0:
            n_wav = n_wav + 0.35 * re_piano_note(hz * 2.0, dur, sr_amb, warmth=0.5)
        end_idx = min(len(save_room), idx + len(n_wav))
        save_room[idx:end_idx] += n_wav[:end_idx - idx] * 0.58
        for d_s, d_g in [(0.42, 0.28), (0.84, 0.14), (1.26, 0.07)]:
            di = idx + int(d_s * sr_amb)
            if di < len(save_room):
                de = min(len(save_room), di + len(n_wav))
                save_room[di:de] += n_wav[:de - di] * d_g
for f_p, g_p in [(73.42, 0.38), (87.31, 0.28), (110.0, 0.25), (130.81, 0.18), (164.81, 0.14)]:
    chorus = 0.004 * np.sin(2 * np.pi * 0.28 * t32 + f_p)
    save_room += g_p * np.sin(2 * np.pi * f_p * (1 + chorus) * t32) * (0.7 + 0.3 * np.cos(2 * np.pi * t32 / 16.0))
write_wav('SaveRoomRE', save_room * env32, sr=sr_amb, loop=True)

# 3. SilentPromiseSH.wav: Silent Hill 2 Akira Yamaoka melancholic guitar & trip-hop pulse (32s @ 22050Hz)
sh_song = np.zeros_like(t32)
sh_chords = [
    (0.0, [164.81, 246.94, 329.63, 369.99, 493.88, 329.63, 246.94, 369.99]),
    (4.0, [130.81, 196.00, 246.94, 329.63, 392.00, 329.63, 246.94, 196.00]),
    (8.0, [110.00, 164.81, 220.00, 246.94, 329.63, 246.94, 220.00, 164.81]),
    (12.0, [123.47, 185.00, 233.08, 293.66, 369.99, 293.66, 233.08, 185.00]),
]
for cycle in [0.0, 16.0]:
    for bar_t, notes_list in sh_chords:
        for step_i, hz in enumerate(notes_list):
            t_on = cycle + bar_t + step_i * 0.5
            idx = int(t_on * sr_amb)
            pl = sh_guitar_pluck(hz, 1.8, sr_amb)
            end_idx = min(len(sh_song), idx + len(pl))
            sh_song[idx:end_idx] += pl[:end_idx - idx] * 0.52
            di = idx + int(0.33 * sr_amb)
            if di < len(sh_song):
                de = min(len(sh_song), di + len(pl))
                sh_song[di:de] += pl[:de - di] * 0.22
for f_p, g_p in [(82.41, 0.30), (123.47, 0.22), (164.81, 0.18), (246.94, 0.12)]:
    sh_song += g_p * np.sin(2 * np.pi * f_p * (1 + 0.0025 * np.sin(2 * np.pi * 0.5 * t32)) * t32) * (0.55 + 0.45 * np.sin(2 * np.pi * t32 / 4.0) ** 2)
write_wav('SilentPromiseSH', sh_song * env32, sr=sr_amb, loop=True)

# 4. HallwayExploreRE.wav: Resident Evil R.P.D. / Mansion dark orchestral suspense exploration (24s @ 22050Hz)
t24 = np.arange(sr_amb * 24) / sr_amb
re_explore = np.zeros_like(t24)
for f_d, g_d, mod_r in [(65.41, 0.38, 0.18), (69.30, 0.22, 0.23), (98.00, 0.24, 0.31), (185.00, 0.12, 0.45), (196.00, 0.14, 0.37)]:
    re_explore += g_d * np.sin(2 * np.pi * f_d * t24 + 0.4 * np.sin(2 * np.pi * mod_r * t24)) * (0.6 + 0.4 * np.sin(2 * np.pi * t24 / 6.0))
pizz_events = [(0.5, 65.41), (2.0, 98.00), (4.5, 69.30), (6.5, 65.41), (9.0, 523.25), (10.5, 493.88), (12.5, 65.41), (15.0, 587.33), (16.5, 554.37), (19.5, 69.30), (21.5, 493.88)]
for pt, phz in pizz_events:
    idx = int(pt * sr_amb)
    pn = re_piano_note(phz, 2.6, sr_amb, warmth=0.6)
    e_idx = min(len(re_explore), idx + len(pn))
    re_explore[idx:e_idx] += pn[:e_idx - idx] * (0.38 if phz > 200 else 0.52)
env24 = np.minimum(t24 / 1.0, 1.0) * np.minimum((24.0 - t24) / 1.2, 1.0)
write_wav('HallwayExploreRE', re_explore * env24, sr=sr_amb, loop=True)

# 5. OtherworldIndustrialSH.wav: Silent Hill Brookhaven Otherworld rhythmic industrial dread (24s @ 22050Hz)
sh_other = np.zeros_like(t24)
for f_o, g_o in [(43.65, 0.42), (65.41, 0.28), (92.50, 0.20), (277.18, 0.09)]:
    sh_other += g_o * np.sin(2 * np.pi * f_o * t24 + 0.8 * np.sin(2 * np.pi * 0.5 * t24))
furnace_noise = sosfilt(butter(2, [85, 680], btype='bandpass', fs=sr_amb, output='sos'), rng.normal(size=len(t24)))
sh_other += furnace_noise * 0.28 * (0.3 + 0.7 * np.sin(2 * np.pi * t24 / 1.5) ** 4)
for hit_i in range(16):
    ht = hit_i * 1.5 + 0.75
    u = np.maximum(0, t24 - ht)
    m = (t24 >= ht).astype(float)
    clang_f = 220.0 if hit_i % 2 == 0 else 277.18
    clang = sum(np.sin(2 * np.pi * f * u) * np.exp(-u * dec) for f, dec in [(clang_f, 4.5), (clang_f * 1.414, 6.5), (clang_f * 2.38, 9.0)])
    sh_other += m * clang * (0.24 if hit_i % 4 == 0 else 0.14)
write_wav('OtherworldIndustrialSH', sh_other * env24, sr=sr_amb, loop=True)

# 6. Hospital.wav, Basement.wav, Wind.wav, Tension.wav: Enhanced Silent Hill / RE room drones (8s @ 22050Hz)
t8 = np.arange(sr_amb * 8) / sr_amb
for name, base_hz, metal_hz in [('Hospital', 48.99, 311.13), ('Basement', 36.71, 233.08), ('Wind', 41.20, 196.00), ('Tension', 55.00, 370.00)]:
    drone = 0.45 * np.sin(2 * np.pi * base_hz * t8) + 0.26 * np.sin(2 * np.pi * (base_hz * 1.5) * t8 + np.sin(2 * np.pi * 0.25 * t8))
    drone += 0.13 * np.sin(2 * np.pi * metal_hz * t8) * (0.5 + 0.5 * np.sin(2 * np.pi * 0.5 * t8))
    rumble = sosfilt(butter(2, [40, 420], btype='bandpass', fs=sr_amb, output='sos'), rng.normal(size=len(t8)))
    drone += rumble * 0.35
    if name in ('Hospital', 'Basement'):
        for hit_t in [1.8, 5.4]:
            u = np.maximum(0, t8 - hit_t)
            mask = (t8 >= hit_t).astype(float)
            clang = sum(np.sin(2 * np.pi * f * u) * np.exp(-u * dec) for f, dec in [(metal_hz, 3.5), (metal_hz * 1.414, 5.0), (metal_hz * 2.73, 8.0)])
            drone += mask * clang * 0.18
    env8 = np.minimum(t8 / 0.6, 1.0) * np.minimum((8.0 - t8) / 0.6, 1.0)
    write_wav(name, drone * env8, sr=sr_amb, loop=True)

# 7. Silent Hill Iconic Radio Static (RadioStatic.wav, 4s loopable @ 48000Hz)
t4 = np.arange(sr48 * 4) / sr48
white = rng.normal(size=len(t4))
band_static = sosfilt(butter(2, [650, 5200], btype='bandpass', fs=sr48, output='sos'), white)
crackle_mod = (0.35 + 0.65 * (np.sin(2 * np.pi * 11 * t4) * np.cos(2 * np.pi * 23 * t4 + 1.2)) ** 4)
carrier_whine = 0.18 * np.sin(2 * np.pi * (1480 + 180 * np.sin(2 * np.pi * 3.5 * t4)) * t4) * (0.5 + 0.5 * np.sin(2 * np.pi * 7 * t4))
write_wav('RadioStatic', band_static * crackle_mod + carrier_whine, sr=sr48, loop=True)

# 8. Silent Hill Air-Raid Siren Warning (SirenWarning.wav, 4.2s @ 48000Hz)
ts = np.arange(int(sr48 * 4.2)) / sr48
f_curve = 195 + 195 * np.sin(np.pi * np.minimum(ts / 3.8, 1.0)) ** 1.3
phase_s = 2 * np.pi * np.cumsum(f_curve) / sr48
siren = sum(g * np.sin(phase_s * h) for h, g in [(1, 1.0), (2, 0.65), (3, 0.35), (4, 0.18), (5, 0.09)])
siren = sosfilt(butter(2, [140, 2400], btype='bandpass', fs=sr48, output='sos'), siren)
write_wav('SirenWarning', siren * (np.sin(np.pi * ts / 4.2) ** 1.2), sr=sr48)

# 9. Resident Evil UI, Typewriter Save, Item Pickup & Door Unlock SFX (@ 48000Hz)
tc = np.arange(int(sr48 * 0.14)) / sr48
re_click = (np.sin(2 * np.pi * 1180 * tc) * np.exp(-tc * 65) + 0.6 * np.sin(2 * np.pi * 2360 * tc) * np.exp(-tc * 95))
write_wav('REMenuClick', re_click, sr=sr48)

tconf = np.arange(int(sr48 * 0.45)) / sr48
re_conf = (np.sin(2 * np.pi * 587.33 * tconf) * np.exp(-tconf * 14) +
           0.85 * (tconf >= 0.07) * np.sin(2 * np.pi * 880.0 * np.maximum(0, tconf - 0.07)) * np.exp(-np.maximum(0, tconf - 0.07) * 9))
write_wav('REMenuConfirm', re_conf, sr=sr48)

tdoor = np.arange(int(sr48 * 1.6)) / sr48
clunk1 = np.sin(2 * np.pi * 140 * tdoor) * np.exp(-tdoor * 28) + 0.5 * sosfilt(butter(2, [300, 2800], btype='bandpass', fs=sr48, output='sos'), rng.normal(size=len(tdoor))) * np.exp(-tdoor * 35)
u2 = np.maximum(0, tdoor - 0.28); m2 = (tdoor >= 0.28).astype(float)
clunk2 = m2 * (np.sin(2 * np.pi * 210 * u2) * np.exp(-u2 * 22) + 0.6 * np.sin(2 * np.pi * 620 * u2) * np.exp(-u2 * 30))
u3 = np.maximum(0, tdoor - 0.55); m3 = (tdoor >= 0.55).astype(float)
slide = m3 * sosfilt(butter(2, [120, 1200], btype='bandpass', fs=sr48, output='sos'), rng.normal(size=len(tdoor))) * np.exp(-u3 * 3.5) * 0.35
write_wav('DoorUnlockRE', clunk1 + clunk2 + slide, sr=sr48)

ttw = np.arange(int(sr48 * 0.55)) / sr48
tw_sig = np.zeros_like(ttw)
for strike_t, s_freq in [(0.0, 1850), (0.09, 2120), (0.19, 1760)]:
    u = np.maximum(0, ttw - strike_t); m = (ttw >= strike_t).astype(float)
    burst = sosfilt(butter(2, [600, 5400], btype='bandpass', fs=sr48, output='sos'), rng.normal(size=len(ttw))) * np.exp(-u * 90)
    tone = np.sin(2 * np.pi * s_freq * u) * np.exp(-u * 75)
    tw_sig += m * (burst * 0.7 + tone * 0.5)
ub = np.maximum(0, ttw - 0.28); mb = (ttw >= 0.28).astype(float)
bell = mb * (0.65 * np.sin(2 * np.pi * 2637 * ub) * np.exp(-ub * 8.5) + 0.35 * np.sin(2 * np.pi * 5274 * ub) * np.exp(-ub * 14.0))
write_wav('TypewriterKeyRE', tw_sig + bell, sr=sr48)

tpick = np.arange(int(sr48 * 0.85)) / sr48
pickup = (np.sin(2 * np.pi * 523.25 * tpick) * np.exp(-tpick * 6.5) +
          0.5 * np.sin(2 * np.pi * 1046.5 * tpick) * np.exp(-tpick * 9.0) +
          (tpick >= 0.11) * 0.85 * np.sin(2 * np.pi * 783.99 * np.maximum(0, tpick - 0.11)) * np.exp(-np.maximum(0, tpick - 0.11) * 4.8) +
          (tpick >= 0.22) * 0.65 * np.sin(2 * np.pi * 1174.66 * np.maximum(0, tpick - 0.22)) * np.exp(-np.maximum(0, tpick - 0.22) * 4.2))
write_wav('ItemPickupRE', pickup, sr=sr48)

tmap = np.arange(int(sr48 * 0.52)) / sr48
paper = sosfilt(butter(2, [450, 4800], btype='bandpass', fs=sr48, output='sos'), rng.normal(size=len(tmap)))
env_map = np.exp(-((tmap - 0.12) / 0.08) ** 2) + 0.65 * np.exp(-((tmap - 0.32) / 0.11) ** 2)
write_wav('MapOpenSH', paper * env_map, sr=sr48)

tmand = np.arange(int(sr48 * 2.2)) / sr48
mand = np.zeros_like(tmand)
for strike_i in range(9):
    st = strike_i * 0.095
    u = np.maximum(0, tmand - st); m = (tmand >= st).astype(float)
    for f_m in [293.66, 311.13, 466.16, 622.25]:
        mand += m * (0.28 / (1 + strike_i * 0.12)) * np.sin(2 * np.pi * f_m * u) * np.exp(-u * 4.2)
write_wav('MandolinStingSH', mand, sr=sr48)

tclang = np.arange(int(sr48 * 2.6)) / sr48
clang_scare = sum(g * np.sin(2 * np.pi * f * tclang) * np.exp(-tclang * dec) for f, g, dec in [
    (118, 0.9, 2.2), (179, 0.7, 2.8), (293, 0.55, 3.4), (468, 0.4, 4.5), (782, 0.25, 6.0), (1340, 0.15, 8.5)
])
clang_burst = sosfilt(butter(2, [180, 2600], btype='bandpass', fs=sr48, output='sos'), rng.normal(size=len(tclang))) * np.exp(-tclang * 14)
write_wav('IndustrialClangSH', clang_scare + clang_burst * 0.45, sr=sr48)

tgroan = np.arange(int(sr48 * 2.8)) / sr48
f_gr = 82.0 - 14.0 * np.sin(np.pi * tgroan / 2.8) + 3.5 * np.sin(2 * np.pi * 6.2 * tgroan)
ph_gr = 2 * np.pi * np.cumsum(f_gr) / sr48
groan_raw = sum((1.0 / k) * np.sin(ph_gr * k) for k in range(1, 16))
groan_filt = sosfilt(butter(2, [90, 950], btype='bandpass', fs=sr48, output='sos'), groan_raw + 0.35 * rng.normal(size=len(tgroan)))
write_wav('MonsterGroanRE', groan_filt * (np.sin(np.pi * tgroan / 2.8) ** 1.3), sr=sr48)

tgurn = np.arange(int(sr48 * 2.5)) / sr48
squeak = np.sin(2 * np.pi * (1420 + 180 * np.sin(2 * np.pi * 3.8 * tgurn)) * tgurn) * (0.5 + 0.5 * np.sin(2 * np.pi * 3.8 * tgurn) ** 4)
roll_rumble = sosfilt(butter(2, [70, 380], btype='bandpass', fs=sr48, output='sos'), rng.normal(size=len(tgurn)))
write_wav('GurneyRollSH', (squeak * 0.35 + roll_rumble * 0.65) * np.sin(np.pi * tgurn / 2.5), sr=sr48)

tbuzz = np.arange(int(sr48 * 2.1)) / sr48
hum = sum(g * np.sin(2 * np.pi * (100 * k) * tbuzz) for k, g in [(1, 0.6), (2, 0.45), (3, 0.35), (5, 0.25), (7, 0.15)])
arc_gate = (np.sin(2 * np.pi * 9.5 * tbuzz) * np.cos(2 * np.pi * 17.0 * tbuzz) > 0.35).astype(float)
arc_noise = sosfilt(butter(2, [1200, 6500], btype='bandpass', fs=sr48, output='sos'), rng.normal(size=len(tbuzz))) * arc_gate
write_wav('FluorescentBuzzSH', (hum * 0.55 + arc_noise * 0.45) * np.sin(np.pi * tbuzz / 2.1), sr=sr48)

print('All Resident Evil & Silent Hill music tracks and sound effects generated.')
