// Dashboard JavaScript functionality

document.addEventListener('DOMContentLoaded', function() {
    
    // Initialize progress bars with colors
    initializeProgressBars();
    
    // Initialize circular progress rings
    initializeCircularProgress();
    
    // Initialize traffic chart
    initializeTrafficChart();
    
    // Add menu item click handlers
    addMenuHandlers();
});

function initializeProgressBars() {
    const progressFills = document.querySelectorAll('.progress-fill');
    
    progressFills.forEach(fill => {
        const color = fill.getAttribute('data-color');
        if (color) {
            fill.style.backgroundColor = color;
        }
        
        // Animate progress bars on load
        const width = fill.style.width;
        fill.style.width = '0%';
        
        setTimeout(() => {
            fill.style.transition = 'width 1s ease-in-out';
            fill.style.width = width;
        }, 500);
    });
}

function initializeCircularProgress() {
    const circularProgresses = document.querySelectorAll('.circular-progress');
    
    circularProgresses.forEach(progress => {
        const percentage = progress.getAttribute('data-percentage');
        const circle = progress.querySelector('.progress-ring-circle');
        const circumference = 2 * Math.PI * 36; // radius is 36
        
        circle.style.strokeDasharray = circumference;
        circle.style.strokeDashoffset = circumference;
        
        // Animate circular progress
        setTimeout(() => {
            const offset = circumference - (percentage / 100) * circumference;
            circle.style.strokeDashoffset = offset;
            circle.style.transition = 'stroke-dashoffset 1.5s ease-in-out';
        }, 800);
    });
}

function initializeTrafficChart() {
    const ctx = document.getElementById('trafficChart');
    if (!ctx) return;
    
    new Chart(ctx, {
        type: 'line',
        data: {
            labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
            datasets: [{
                label: 'Page Views',
                data: [1200, 1900, 1500, 2200, 2800, 2100, 2400],
                borderColor: '#ff6b81',
                backgroundColor: 'rgba(255, 107, 129, 0.1)',
                fill: true,
                tension: 0.4
            }, {
                label: 'Unique Visitors',
                data: [800, 1200, 1000, 1500, 1800, 1400, 1600],
                borderColor: '#5f27cd',
                backgroundColor: 'rgba(95, 39, 205, 0.1)',
                fill: true,
                tension: 0.4
            }, {
                label: 'Sessions',
                data: [600, 900, 750, 1100, 1300, 1000, 1200],
                borderColor: '#00d2d3',
                backgroundColor: 'rgba(0, 210, 211, 0.1)',
                fill: true,
                tension: 0.4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    grid: {
                        color: 'rgba(0, 0, 0, 0.05)'
                    }
                },
                x: {
                    grid: {
                        display: false
                    }
                }
            },
            elements: {
                point: {
                    radius: 4,
                    hoverRadius: 6
                }
            }
        }
    });
}

function addMenuHandlers() {
    const menuItems = document.querySelectorAll('.menu-item');
    
    menuItems.forEach(item => {
        item.addEventListener('click', function() {
            // Remove active class from all items
            menuItems.forEach(mi => mi.classList.remove('active'));
            
            // Add active class to clicked item
            this.classList.add('active');
            
            // You can add routing logic here
            const menuText = this.querySelector('span').textContent;
            console.log(`Navigating to: ${menuText}`);
        });
    });
    
    // Handle nav links
    const navLinks = document.querySelectorAll('.nav-link');
    navLinks.forEach(link => {
        link.addEventListener('click', function(e) {
            e.preventDefault();
            
            // Remove active class from all nav links
            navLinks.forEach(nl => nl.classList.remove('active'));
            
            // Add active class to clicked link
            this.classList.add('active');
            
            console.log(`Navigating to: ${this.textContent}`);
        });
    });
    
    // Handle reply buttons
    const replyButtons = document.querySelectorAll('.reply-btn');
    replyButtons.forEach(btn => {
        btn.addEventListener('click', function() {
            console.log('Reply button clicked');
            // Add reply functionality here
        });
    });
}

// Add some animations for visited items
function animateVisitedItems() {
    const visitedItems = document.querySelectorAll('.visited-item');
    
    visitedItems.forEach((item, index) => {
        item.style.opacity = '0';
        item.style.transform = 'translateX(-20px)';
        
        setTimeout(() => {
            item.style.transition = 'all 0.5s ease';
            item.style.opacity = '1';
            item.style.transform = 'translateX(0)';
        }, index * 100);
    });
}

// Call animation after DOM is loaded
setTimeout(animateVisitedItems, 1000);
