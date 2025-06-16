window.downloadFile = (filename, content) => {
    const blob = new Blob([content], { type: 'text/csv;charset=utf-8;' });
    if (window.navigator && window.navigator.msSaveBlob) {
        window.navigator.msSaveBlob(blob, filename);
    } else {
        const link = document.createElement('a');
        link.href = URL.createObjectURL(blob);
        link.download = filename;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    }
};
