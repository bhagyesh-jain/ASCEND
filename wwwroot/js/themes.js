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
      count = 70; // More stars
    } else if (this.theme === 'Sakura Ascension') {
      count = 35; // Soft cherry blossoms
    } else if (this.theme === 'Forest Guardian') {
      count = 30; // Magical leaves
    }

    for (let i = 0; i < count; i++) {
      this.particles.push(this.createParticle(true));
    }
  }

  createParticle(randomY = false) {
    const w = this.canvas.width;
    const h = this.canvas.height;
    const y = randomY ? Math.random() * h : (this.theme === 'Shadow Hunter' ? h + 10 : -10);

    switch (this.theme) {
      case 'Shadow Hunter':
        return {
          x: Math.random() * w,
          y: y,
          size: Math.random() * 3 + 1,
          speedY: -(Math.random() * 1.5 + 0.5),
          speedX: (Math.random() - 0.5) * 0.5,
          alpha: Math.random() * 0.5 + 0.2,
          color: Math.random() > 0.5 ? '#00CFFF' : '#6A5ACD'
        };

      case 'Sakura Ascension':
        return {
          x: Math.random() * w,
          y: randomY ? Math.random() * h : -20,
          size: Math.random() * 6 + 4,
          speedY: Math.random() * 1.2 + 0.8,
          speedX: Math.random() * 1.5 + 0.5, // Drift right
          angle: Math.random() * 360,
          spin: (Math.random() - 0.5) * 2,
          alpha: Math.random() * 0.6 + 0.3,
          color: '#FF7EB6'
        };

      case 'Celestial Realm':
        return {
          x: Math.random() * w,
          y: Math.random() * h,
          size: Math.random() * 2 + 0.5,
          twinkleSpeed: Math.random() * 0.02 + 0.005,
          alpha: Math.random(),
          phase: Math.random() * Math.PI * 2
        };

      case 'Forest Guardian':
        return {
          x: Math.random() * w,
          y: randomY ? Math.random() * h : -20,
          size: Math.random() * 7 + 3,
          speedY: Math.random() * 0.8 + 0.5,
          speedX: (Math.random() - 0.5) * 1,
          angle: Math.random() * Math.PI * 2,
          spin: (Math.random() - 0.5) * 0.02,
          alpha: Math.random() * 0.6 + 0.2,
          color: Math.random() > 0.6 ? '#4CAF50' : '#81C784'
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
      this.ctx.shadowBlur = 8;
      this.ctx.shadowColor = p.color;
      this.ctx.fillStyle = p.color;
      this.ctx.globalAlpha = p.alpha;
      this.ctx.beginPath();
      this.ctx.arc(p.x, p.y, p.size, 0, Math.PI * 2);
      this.ctx.fill();
    } 
    else if (this.theme === 'Sakura Ascension') {
      this.ctx.translate(p.x, p.y);
      this.ctx.rotate((p.angle * Math.PI) / 180);
      this.ctx.fillStyle = p.color;
      this.ctx.globalAlpha = p.alpha;
      
      // Draw a cherry blossom petal (leaf shape)
      this.ctx.beginPath();
      this.ctx.moveTo(0, 0);
      this.ctx.quadraticCurveTo(-p.size, -p.size/2, -p.size, -p.size);
      this.ctx.quadraticCurveTo(-p.size/2, -p.size * 1.5, 0, -p.size * 1.8);
      this.ctx.quadraticCurveTo(p.size/2, -p.size * 1.5, p.size, -p.size);
      this.ctx.quadraticCurveTo(p.size, -p.size/2, 0, 0);
      this.ctx.fill();
    } 
    else if (this.theme === 'Celestial Realm') {
      // Twinkling star
      p.phase += p.twinkleSpeed;
      const alpha = Math.abs(Math.sin(p.phase));
      
      this.ctx.fillStyle = '#FFFFFF';
      this.ctx.globalAlpha = alpha * 0.8;
      this.ctx.beginPath();
      this.ctx.arc(p.x, p.y, p.size, 0, Math.PI * 2);
      this.ctx.fill();
    } 
    else if (this.theme === 'Forest Guardian') {
      this.ctx.translate(p.x, p.y);
      this.ctx.rotate(p.angle);
      this.ctx.fillStyle = p.color;
      this.ctx.globalAlpha = p.alpha;

      // Draw leaf shape
      this.ctx.beginPath();
      this.ctx.moveTo(0, 0);
      this.ctx.quadraticCurveTo(-p.size/2, -p.size/2, 0, -p.size);
      this.ctx.quadraticCurveTo(p.size/2, -p.size/2, 0, 0);
      this.ctx.fill();
    }

    this.ctx.restore();
  }

  updateParticle(p, index) {
    if (this.theme === 'Celestial Realm') {
      // Stars don't move, they just twinkle (handled in draw)
      return;
    }

    p.y += p.speedY;
    p.x += p.speedX;

    if (p.spin) {
      if (this.theme === 'Sakura Ascension') {
        p.angle += p.spin;
      } else if (this.theme === 'Forest Guardian') {
        p.angle += p.spin;
      }
    }

    // Recycle off-screen particles
    const isOffScreen = 
      p.y > this.canvas.height + 20 || 
      p.y < -20 || 
      p.x > this.canvas.width + 20 || 
      p.x < -20;

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
