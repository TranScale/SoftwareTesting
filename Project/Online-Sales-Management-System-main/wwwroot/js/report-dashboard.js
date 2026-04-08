// File: wwwroot/js/report-dashboard.js

function initComparisonChart(canvasId, labels, dataSales, dataPurchases) {
    const ctx = document.getElementById(canvasId);

    if (!ctx) return;

    // Màu sắc theo theme
    const colorSales = '#10b981'; // Xanh lá
    const colorCost = '#ef4444';  // Đỏ
    const colorGrid = '#f3f4f6';  // Lưới mờ

    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Doanh thu',
                    data: dataSales,
                    backgroundColor: colorSales,
                    borderRadius: 4,
                    barPercentage: 0.6,
                    categoryPercentage: 0.8
                },
                {
                    label: 'Chi phí',
                    data: dataPurchases,
                    backgroundColor: colorCost,
                    borderRadius: 4,
                    barPercentage: 0.6,
                    categoryPercentage: 0.8
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'top',
                    align: 'end',
                    labels: {
                        usePointStyle: true,
                        boxWidth: 8,
                        font: { size: 12, weight: '600', family: "'Segoe UI', sans-serif" }
                    }
                },
                tooltip: {
                    backgroundColor: '#1e293b',
                    padding: 12,
                    titleFont: { size: 13 },
                    bodyFont: { size: 13 },
                    cornerRadius: 8,
                    displayColors: true,
                    callbacks: {
                        label: function (context) {
                            let label = context.dataset.label || '';
                            if (label) {
                                label += ': ';
                            }
                            if (context.parsed.y !== null) {
                                // Format tiền tệ VN (ví dụ: 1,000,000)
                                label += new Intl.NumberFormat('vi-VN').format(context.parsed.y);
                            }
                            return label;
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    grid: {
                        color: colorGrid,
                        borderDash: [5, 5]
                    },
                    ticks: {
                        font: { size: 11 },
                        color: '#64748b'
                    },
                    border: { display: false }
                },
                x: {
                    grid: { display: false },
                    ticks: {
                        font: { size: 11 },
                        color: '#64748b'
                    },
                    border: { display: false }
                }
            },
            interaction: {
                mode: 'index',
                intersect: false,
            },
        }
    });
}