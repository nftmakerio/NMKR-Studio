function openPaymentWindow(paymentUrl) {
    // Specify the popup width and height
    const popupWidth = 500;
    const popupHeight = 700;

    // Calculate the center of the screen
    const left = window.top.outerWidth / 2 + window.top.screenX - (popupWidth / 2);
    const top = window.top.outerHeight / 2 + window.top.screenY - (popupHeight / 2);

    const popup = window.open(paymentUrl, "NMKR Pay", `popup=1, location=1, width=${popupWidth}, height=${popupHeight}, left=${left}, top=${top}`);

    // Show dim background
    document.body.style = "background: rgba(0, 0, 0, 0.5)";

    // Continuously check whether the popup has been closed
    const backgroundCheck = setInterval(function () {
        if (popup.closed) {
            clearInterval(backgroundCheck);

            console.log("Popup closed");

            // Remove dim background
            document.body.style = "";
        }
    }, 1000);
}


function openWindow(paymentUrl) {
    const popup = window.open(paymentUrl, "_blank");
}

function SetPlainDetails(fullname, shortname, customerid) {

    Plain.init({
        customerDetails: {
            fullName: fullname,
            shortName: shortname,
            externalid: customerid
        },
        appId: 'liveChatApp_01JAFM75T0VXH1PM8Y7N08M47Q',
        title: 'Welcome to the NMKR Support!',
        theme: 'light',
        style: {
            chatButtonColor: '#000000',
            chatButtonIconColor: '#11F250',
        },
        links: [
            {
                icon: 'book',
                text: 'NMKR Docs',
                url: 'https://docs.nmkr.io',
            },
            {
                text: 'Watch our 3 Minute Onboarding Video Tutorial',
                url: 'https://docs.nmkr.io/nmkr-studio/learn-nmkr-studio-in-3-minutes',
            }
        ],
    });
}