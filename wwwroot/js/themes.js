// ==========================================
// ASCEND FANTASY THEME PARTICLE ENGINE
// ==========================================

class ThemeParticleEngine {
  constructor() {
    this.canvas = document.getElementById('theme-canvas');
    if (!this.canvas) return;
    this.ctx = this.canvas.getContext('2d');
    this.particles = [];
    this.animationFrameId = null;
    this.theme = document.body.getAttribute('data-theme') || 'Shadow Hunter';

    this.resizeCanvas();
    window.addEventListener('resize', () => this.resizeCanvas());

    // Observe theme changes on body
    const observer = new MutationObserver((mutations) => {
      mutations.forEach((mutation) => {
        if (mutation.type === 'attributes' && mutation.attributeName === 'data-theme') {
          const newTheme = document.body.getAttribute('data-theme');
          if (newTheme !== this.theme) {
            this.theme = newTheme;
            this.initParticles();
          }
        }
      });
    });
    observer.observe(document.body, { attributes: true });

    this.initParticles();
    this.animate();
  }

  resizeCanvas() {
    this.canvas.width = window.innerWidth;
    this.canvas.height = window.innerHeight;
    this.initParticles();
  }

  initParticles() {
    this.particles = [];
    let count = 40;

    if (this.theme === 'Celestial Realm') {
      count = 80; // Cosmic starry sky
    } else if (this.theme === 'Sakura Ascension') {
      count = 35; // Gentle petals
    } else if (this.theme === 'Forest Guardian') {
      count = 45; // Leaves + golden fireflies
    } else if (this.theme === 'Shadow Hunter') {
      count = 40; // Aura flames
    }

    for (let i = 0; i < count; i++) {
      this.particles.push(this.createParticle(true));
    }
  }

  createParticle(randomY = false) {
    const w = this.canvas.width;
    const h = this.canvas.height;
    const y = randomY ? Math.random() * h : (this.theme === 'Shadow Hunter' ? h + 20 : -20);

    switch (this.theme) {
      case 'Shadow Hunter':
        return {
          type: 'flame',
          x: Math.random() * w,
          y: y,
          size: Math.random() * 4 + 1.5,
          speedY: -(Math.random() * 1.8 + 0.6),
          speedX: (Math.random() - 0.5) * 0.8,
          alpha: Math.random() * 0.6 + 0.2,
          color: Math.random() > 0.45 ? '#00CFFF' : '#A855F7',
          wobbleSpeed: Math.random() * 0.05 + 0.01,
          wobbleRange: Math.random() * 20 + 5,
          phase: Math.random() * Math.PI
        };

      case 'Sakura Ascension':
        return {
          type: 'petal',
          x: Math.random() * w,
          y: randomY ? Math.random() * h : -20,
          size: Math.random() * 7 + 4,
          speedY: Math.random() * 1.0 + 0.6,
          speedX: Math.random() * 1.2 + 0.3, // Drift right
          angle: Math.random() * Math.PI * 2,
          spin: (Math.random() - 0.5) * 0.025,
          alpha: Math.random() * 0.7 + 0.3,
          color: Math.random() > 0.7 ? '#FF7EB6' : '#FFD6E8',
          wobble: Math.random() * Math.PI,
          wobbleSpeed: Math.random() * 0.02 + 0.01
        };

      case 'Celestial Realm':
        // Twinkling stars + rare shooting star
        const isShootingStar = Math.random() > 0.97;
        return {
          type: isShootingStar ? 'shooting' : 'star',
          x: Math.random() * w,
          y: Math.random() * h,
          size: isShootingStar ? Math.random() * 2 + 1 : Math.random() * 2.2 + 0.4,
          twinkleSpeed: Math.random() * 0.025 + 0.008,
          alpha: Math.random() * 0.8 + 0.2,
          phase: Math.random() * Math.PI * 2,
          speedX: isShootingStar ? -(Math.random() * 8 + 4) : 0,
          speedY: isShootingStar ? Math.random() * 8 + 4 : 0,
          life: isShootingStar ? 1.0 : 0
        };

      case 'Forest Guardian':
        const isFirefly = Math.random() > 0.5;
        return {
          type: isFirefly ? 'firefly' : 'leaf',
          x: Math.random() * w,
          y: randomY ? Math.random() * h : -20,
          size: isFirefly ? Math.random() * 2 + 1 : Math.random() * 8 + 3,
          speedY: isFirefly ? (Math.random() - 0.5) * 0.4 - 0.1 : Math.random() * 0.8 + 0.4,
          speedX: isFirefly ? (Math.random() - 0.5) * 0.8 : (Math.random() - 0.5) * 0.6,
          angle: Math.random() * Math.PI * 2,
          spin: (Math.random() - 0.5) * 0.02,
          alpha: Math.random() * 0.65 + 0.25,
          color: isFirefly ? '#F59E0B' : (Math.random() > 0.6 ? '#10B981' : '#059669'),
          wobble: Math.random() * Math.PI,
          wobbleSpeed: Math.random() * 0.03 + 0.01
        };

      default:
        return {
          x: Math.random() * w,
          y: Math.random() * h,
          size: 2,
          speedY: -1,
          speedX: 0,
          alpha: 0.5
        };
    }
  }

  drawParticle(p) {
    this.ctx.save();

    if (this.theme === 'Shadow Hunter') {
      this.ctx.shadowBlur = 10;
      this.ctx.shadowColor = p.color;
      this.ctx.fillStyle = p.color;
      this.ctx.globalAlpha = p.alpha;
      this.ctx.beginPath();
      // Draw a flame-like oval/tear
      this.ctx.arc(p.x, p.y, p.size, 0, Math.PI * 2);
      this.ctx.fill();
    } 
    else if (this.theme === 'Sakura Ascension') {
      this.ctx.translate(p.x, p.y);
      this.ctx.rotate(p.angle);
      this.ctx.fillStyle = p.color;
      this.ctx.globalAlpha = p.alpha;
      
      // Draw cherry blossom petal
      this.ctx.beginPath();
      this.ctx.moveTo(0, 0);
      this.ctx.quadraticCurveTo(-p.size, -p.size/2, -p.size, -p.size);
      this.ctx.quadraticCurveTo(-p.size/2, -p.size * 1.5, 0, -p.size * 1.8);
      this.ctx.quadraticCurveTo(p.size/2, -p.size * 1.5, p.size, -p.size);
      this.ctx.quadraticCurveTo(p.size, -p.size/2, 0, 0);
      this.ctx.fill();
    } 
    else if (this.theme === 'Celestial Realm') {
      if (p.type === 'shooting') {
        // Draw shooting star trail
        const gradient = this.ctx.createLinearGradient(p.x, p.y, p.x - p.speedX * 3, p.y - p.speedY * 3);
        gradient.addColorStop(0, `rgba(224, 170, 255, ${p.life})`);
        gradient.addColorStop(1, 'rgba(224, 170, 255, 0)');
        this.ctx.strokeStyle = gradient;
        this.ctx.lineWidth = p.size;
        this.ctx.lineCap = 'round';
        this.ctx.beginPath();
        this.ctx.moveTo(p.x, p.y);
        this.ctx.lineTo(p.x - p.speedX * 3, p.y - p.speedY * 3);
        this.ctx.stroke();
      } else {
        // Twinkling star
        this.ctx.fillStyle = '#E0E7FF';
        this.ctx.globalAlpha = p.alpha * 0.85;
        this.ctx.beginPath();
        this.ctx.arc(p.x, p.y, p.size, 0, Math.PI * 2);
        this.ctx.fill();
      }
    } 
    else if (this.theme === 'Forest Guardian') {
      if (p.type === 'firefly') {
        // Glowing gold firefly
        this.ctx.shadowBlur = 12;
        this.ctx.shadowColor = p.color;
        this.ctx.fillStyle = p.color;
        this.ctx.globalAlpha = p.alpha;
        this.ctx.beginPath();
        this.ctx.arc(p.x, p.y, p.size, 0, Math.PI * 2);
        this.ctx.fill();
      } else {
        // Leaf shape
        this.ctx.translate(p.x, p.y);
        this.ctx.rotate(p.angle);
        this.ctx.fillStyle = p.color;
        this.ctx.globalAlpha = p.alpha;

        this.ctx.beginPath();
        this.ctx.moveTo(0, 0);
        this.ctx.quadraticCurveTo(-p.size/2, -p.size/2, 0, -p.size);
        this.ctx.quadraticCurveTo(p.size/2, -p.size/2, 0, 0);
        this.ctx.fill();
      }
    }

    this.ctx.restore();
  }

  updateParticle(p, index) {
    if (this.theme === 'Celestial Realm' && p.type === 'star') {
      p.phase += p.twinkleSpeed;
      p.alpha = Math.abs(Math.sin(p.phase));
      return;
    }

    if (this.theme === 'Celestial Realm' && p.type === 'shooting') {
      p.x += p.speedX;
      p.y += p.speedY;
      p.life -= 0.02; // Decay life
      
      if (p.life <= 0 || p.x < -50 || p.y > this.canvas.height + 50) {
        this.particles[index] = this.createParticle(false);
      }
      return;
    }

    // Standard particle movement
    p.y += p.speedY;
    
    if (this.theme === 'Shadow Hunter') {
      p.phase += p.wobbleSpeed;
      p.x += p.speedX + Math.sin(p.phase) * 0.25;
    } else if (this.theme === 'Sakura Ascension') {
      p.wobble += p.wobbleSpeed;
      p.x += p.speedX + Math.sin(p.wobble) * 0.4;
      p.angle += p.spin;
    } else if (this.theme === 'Forest Guardian') {
      if (p.type === 'firefly') {
        p.wobble += p.wobbleSpeed;
        p.x += p.speedX + Math.sin(p.wobble) * 0.3;
        // Keep fireflies bouncing slowly up and down
        p.speedY = Math.max(-0.4, Math.min(0.2, p.speedY + (Math.random() - 0.5) * 0.05));
      } else {
        p.wobble += p.wobbleSpeed;
        p.x += p.speedX + Math.sin(p.wobble) * 0.3;
        p.angle += p.spin;
      }
    }

    // Recycle off-screen particles
    const isOffScreen = 
      p.y > this.canvas.height + 30 || 
      p.y < -30 || 
      p.x > this.canvas.width + 30 || 
      p.x < -30;

    if (isOffScreen) {
      this.particles[index] = this.createParticle(false);
    }
  }

  animate() {
    this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);

    for (let i = 0; i < this.particles.length; i++) {
      const p = this.particles[i];
      this.drawParticle(p);
      this.updateParticle(p, i);
    }

    this.animationFrameId = requestAnimationFrame(() => this.animate());
  }
}

// Instantiate on load
document.addEventListener('DOMContentLoaded', () => {
  window.themeEngine = new ThemeParticleEngine();
});
