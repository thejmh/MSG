const fs = require('fs');
const path = require('path');

const csvPath = 'c:/Users/isaac/Downloads/MSG/UnityProject/Assets/Resources/Acupoints.csv';
const outputPath = 'c:/Users/isaac/Downloads/MSG/public/acupoints.json';

const content = fs.readFileSync(csvPath, 'utf8');
const lines = content.split('\n');

const headers = lines[0].split(',');
const acupoints = [];

// Helper to determine region and relative coordinates (X, Y in %) for visual maps
function getRegionAndCoords(id, meridian, name) {
  id = parseInt(id);
  // Default fallbacks
  let region = 'head';
  let x = 50;
  let y = 50;

  // Let's classify based on Meridian and ID ranges
  if (meridian.includes('태음폐경')) {
    region = 'arm';
    // Mapping 1~11 along the inner arm
    x = 30 + (id - 1) * 5;
    y = 40 + (id - 1) * 3;
  } else if (meridian.includes('양명대장경')) {
    if (id <= 27) {
      region = 'arm';
      x = 40 + (id - 12) * 3;
      y = 35 + (id - 12) * 2;
    } else {
      region = 'head';
      if (name.includes('영향')) { x = 48; y = 52; }
      else if (name.includes('화료')) { x = 49; y = 55; }
      else { x = 45; y = 60; }
    }
  } else if (meridian.includes('양명위경')) {
    if (id <= 42) {
      region = 'head';
      if (name.includes('사백')) { x = 45; y = 43; }
      else { x = 46; y = 50; }
    } else if (id <= 61) {
      region = 'front_body';
      x = 40;
      y = 30 + (id - 43) * 3;
    } else {
      region = 'leg';
      // 62~76
      x = 45;
      y = 30 + (id - 62) * 4;
    }
  } else if (meridian.includes('태음비경')) {
    if (id <= 88) {
      region = 'leg';
      x = 55;
      y = 80 - (id - 77) * 4;
    } else {
      region = 'front_body';
      x = 60;
      y = 50 + (id - 89) * 4;
    }
  } else if (meridian.includes('소음심경')) {
    region = 'arm';
    x = 25 + (id - 98) * 6;
    y = 55 + (id - 98) * 3;
  } else if (meridian.includes('태양소장경')) {
    if (id <= 114) {
      region = 'arm';
      x = 35 + (id - 107) * 5;
      y = 65 - (id - 107) * 2;
    } else if (id <= 119) {
      region = 'back_body';
      x = 35 + (id - 115) * 4;
      y = 25;
    } else {
      region = 'head';
      x = 35; y = 40 + (id - 120) * 3;
    }
  } else if (meridian.includes('태양방광경')) {
    if (id <= 135) {
      region = 'head';
      if (name.includes('천주')) { x = 50; y = 82; }
      else { x = 50; y = 30; }
    } else if (id <= 160) {
      region = 'back_body';
      x = 45;
      y = 20 + (id - 136) * 2.5;
    } else {
      region = 'leg';
      x = 50;
      y = 40 + (id - 161) * 2;
    }
  } else if (meridian.includes('소음신경')) {
    if (id <= 202) {
      region = 'leg';
      x = 52;
      y = 90 - (id - 193) * 4;
    } else {
      region = 'front_body';
      x = 48;
      y = 40 + (id - 203) * 3;
    }
  } else if (meridian.includes('궐음심포경')) {
    if (id === 220) {
      region = 'front_body';
      x = 35; y = 40;
    } else {
      region = 'arm';
      x = 20 + (id - 221) * 7;
      y = 50 + (id - 221) * 2;
    }
  } else if (meridian.includes('소양삼초경')) {
    if (id <= 241) {
      region = 'arm';
      x = 25 + (id - 229) * 5;
      y = 45 + (id - 229) * 2;
    } else if (id <= 243) {
      region = 'back_body';
      x = 30; y = 20 + (id - 242) * 5;
    } else {
      region = 'head';
      x = 58; y = 45 + (id - 244) * 3;
    }
  } else if (meridian.includes('소양담경')) {
    if (id <= 271) {
      region = 'head';
      if (name.includes('풍지')) { x = 60; y = 80; }
      else if (name.includes('솔곡')) { x = 62; y = 35; }
      else { x = 65; y = 45; }
    } else if (id === 272) {
      region = 'back_body';
      x = 28; y = 22; // 견정
    } else if (id <= 279) {
      region = 'front_body';
      x = 65; y = 35 + (id - 273) * 4;
    } else {
      region = 'leg';
      x = 42; y = 35 + (id - 280) * 4;
    }
  } else if (meridian.includes('궐음간경')) {
    if (id <= 304) {
      region = 'leg';
      x = 58; y = 85 - (id - 296) * 5;
    } else {
      region = 'front_body';
      x = 55; y = 60 + (id - 305) * 4;
    }
  } else if (meridian.includes('임맥')) {
    if (id <= 331) {
      region = 'front_body';
      x = 50; // Midline
      y = 85 - (id - 310) * 2.5;
    } else {
      region = 'head';
      x = 50; y = 70 + (id - 332) * 10;
    }
  } else if (meridian.includes('독맥')) {
    if (id <= 336) {
      region = 'back_body';
      x = 50; y = 80 - (id - 334) * 5;
    } else if (id <= 347) {
      region = 'back_body';
      x = 50; // Midline back
      y = 70 - (id - 337) * 4.5;
    } else {
      region = 'head';
      x = 50;
      if (name.includes('백회')) { x = 50; y = 18; }
      else { y = 15 + (id - 348) * 3; }
    }
  }

  // Adjust coordinates specifically for the 49 key prescription points to make them perfectly visually aligned
  switch(id) {
    case 15: // 합곡
      region = 'arm'; x = 70; y = 35; break;
    case 353: // 백회
      region = 'head'; x = 50; y = 15; break;
    case 67: // 족삼리
      region = 'leg'; x = 40; y = 50; break;
    case 271: // 풍지
      region = 'head'; x = 58; y = 76; break;
    case 225: // 내관
      region = 'arm'; x = 42; y = 45; break;
    case 82: // 삼음교
      region = 'leg'; x = 58; y = 70; break;
    case 22: // 곡지
      region = 'arm'; x = 32; y = 55; break;
    case 272: // 견정 (GB21)
      region = 'back_body'; x = 32; y = 20; break;
    case 347: // 대추
      region = 'back_body'; x = 50; y = 14; break;
    case 183: // 승산
      region = 'leg'; x = 50; y = 62; break;
    case 165: // 위중
      region = 'leg'; x = 50; y = 42; break;
    case 148: // 신유
      region = 'back_body'; x = 46; y = 64; break;
    case 321: // 중완
      region = 'front_body'; x = 50; y = 45; break;
    case 326: // 단중
      region = 'front_body'; x = 50; y = 30; break;
    case 33: // 사백
      region = 'head'; x = 50; y = 45; break;
    case 31: // 영향
      region = 'head'; x = 46; y = 53; break;
  }

  return { region, x: parseFloat(x.toFixed(1)), y: parseFloat(y.toFixed(1)) };
}

for (let i = 1; i < lines.length; i++) {
  const line = lines[i].trim();
  if (!line) continue;
  
  // Custom split that handles commas inside quotes (symptoms, locations)
  const cols = [];
  let current = '';
  let inQuotes = false;
  
  for (let c = 0; c < line.length; c++) {
    const char = line[c];
    if (char === '"') {
      inQuotes = !inQuotes;
    } else if (char === ',' && !inQuotes) {
      cols.push(current.trim());
      current = '';
    } else {
      current += char;
    }
  }
  cols.push(current.trim());

  if (cols.length >= 8) {
    const id = cols[0];
    const meridian = cols[1];
    const name = cols[2];
    const hanja = cols[3];
    const page = cols[4];
    const symptoms = cols[5].replace(/^"|"$/g, '');
    const priority = cols[6];
    const location = cols[7].replace(/^"|"$/g, '');

    const visual = getRegionAndCoords(id, meridian, name);

    acupoints.push({
      id: parseInt(id),
      meridian,
      name,
      hanja,
      page: parseInt(page) || null,
      symptoms,
      priority,
      location,
      region: visual.region,
      x: visual.x,
      y: visual.y
    });
  }
}

fs.writeFileSync(outputPath, JSON.stringify(acupoints, null, 2), 'utf8');
console.log(`Parsed ${acupoints.length} acupoints and wrote to ${outputPath}`);
