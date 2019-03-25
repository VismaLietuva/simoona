/*!
 *
 * Small scripts needed for browser detection ,navigations or tooltips
 * 
 */
 
// Add ie10 class for the html tag in IE10
if (Function('/*@cc_on return document.documentMode===10@*/')()){
	document.documentElement.className+=' ie10';
} 

// Add ie11 class for the html tag in IE11
jQuery(window).load(function() {
	if(navigator.userAgent.match(/Trident.*rv:11\./)) {
		jQuery('html').addClass('ie11');
	}
});

// IE bug for buttons
$(window).on('load', function(){ 
	if (!!navigator.userAgent.match(/Trident\/7\./)){
		$(function() {
			$('button.close').wrapInner('<span></span>');
			$('.btn:not(.left):not(.right):not(.go2first):not(.go2last):not(.prev):not(.next)').wrapInner('<span></span>');
			$('.btn:not(.left):not(.right) span.caret').each(function() {
				$(this).appendTo($(this).parent().parent());
			});
		});
	}
});
	
	
	
//----------------- Main navigation
	// navbar-form - focus state

	$(document).on("focus", ".navbar-form > .form-group > input[type='text']", function(){
		//$(this).parent().addClass("focus");
	});
	$(document).on("blur", ".navbar-form > .form-group > input[type='text']", function(){
		//$(this).parent().removeClass("focus");
	});
	
	// disable the collapsing effect
	$('.navbar-default > .collapse').on('show.bs.collapse hide.bs.collapse', function(e) {
		e.preventDefault();
	});
	$('.navbar-default > .navbar-header > [data-toggle="collapse"]').on('click', function(e) {
		e.preventDefault();
		$($(this).data('target')).toggleClass('in');
	});
	
	
	//Move second-level up and down in the HTML structure, depending on your screen size
	$(window).on('load', function(){
		if ($(window).width() > 950) {
			$(".first-level > li.active > .second-level").each( function() {
				$(this).appendTo($(this).parent().parent().parent());
			});
		} else {
			$(".first-level > li.active > .second-level").each( function() {
				$(this).appendTo($(this).parent().parent().parent());
			});
		}
	});	
	
	// Navbars - active class for each element from menu when selected 
	if ($(window).width() > 949) {
		$('.navbar #main-navigation .navbar-nav > li').click(function() {
			$('.navbar #main-navigation .navbar-nav > li').removeClass('active');
			$(this).addClass('active');
		}); 
	}

	// Clone the active menu item and display on mobile
	$(window).on('load', function(){
		var $lvl1 = $('.navbar-nav > .active > a').text();   
		var $lvl2 = $('.navbar-nav .collapse .active').text();  
		if ($('.collapse li a').hasClass('active')) {
			$('.active-on-mobile').append($lvl2);
		}
		else {
			$('.active-on-mobile').append($lvl1);
		}
	});
	
	$('ul.second-level.collapse > li.dropdown-element > a').on('show.bs.dropdown', function () {
	  // do something…
	})

	// Keep the subnav with the active item open
	$(window).on('load', function(){
		if ($('.third-level > li > a').hasClass('active')) {
			$(this).parent().parent().parent().addClass('active-child-third-lvl');
			if ($(window).width() < 949) {
				$('ul.third-level.collapse').addClass('in').prev().addClass('open-sublevel');  
				$('ul.second-level.collapse').addClass('in').prev().addClass('open-sublevel');  
			}
		}
		
		if ($('.second-level > li > a').hasClass('active')) {
			if ($(window).width() < 949) {
				$('ul.second-level.collapse').addClass('in').prev().addClass('open-sublevel');  
			}
		}
		
		if ($('.first-level > li').hasClass('active')) { 
			if ($(window).width() < 949) {
				$('li.active > ul.second-level.collapse').addClass('in').prev().addClass('open-sublevel');  
			}
		}
	});

	// when screen smaller then 949px, add 'closed-navigation' class on parent class
	$('.navbar.navbar-default').addClass('closed-navigation');  
	$('.navbar-header .navbar-toggle').click(function(){
		if ($('.navbar-collapse.collapse').hasClass('in')) {
			$('.navbar.navbar-default').addClass('closed-navigation');  
		} else { 
			$('.navbar.navbar-default').removeClass('closed-navigation'); 
		}
	});
	$('body > div').click(function(){
		$('.navbar.navbar-default').addClass('closed-navigation');  
	});

	// just one active element for screen smaller then 949px
	$(window).click(function(){
		if ($('.collapse > li > a').hasClass('active')) {
			if ($(window).width() < 949) {
				$('.first-level > li').removeClass('active');  
				$('.dropdown-element').removeClass('activeParent');  
			}
		}
	});

	// add hover class for dropdown-element and no hover effect if there is an empty href
	// just for the case when the window is smaller then 949px
	if ($(window).width() < 949) {
		if ($('.dropdown-element > a:first-child').attr('href') != '') { 
			$('.dropdown-element > a').hover(
				function(){ $(this).parent().addClass('noHoverState') },
				function(){ $(this).parent().removeClass('noHoverState') }
			)
		} else {
			$('.dropdown-element > a').hover(
				function(){ $(this).parent().addClass('hoverState') },
				function(){ $(this).parent().removeClass('hoverState') }
			)
		}
	} else {
		$('.dropdown-element > a').hover(
			function(){ $(this).parent().addClass('hoverState') },
			function(){ $(this).parent().removeClass('hoverState') }
		)
	}
	
	// add focus on the dropdown element from the second level navigation
	$(".second-level-collapse > li.dropdown-element > a").focus(function(){
		$('.second-level-collapse > li.dropdown-element > a').parent().addClass("focusElem");
	}).blur(function(){
		$('.second-level-collapse > li.dropdown-element > a').parent().removeClass("focusElem");
	})
	
	// Enable CSS active pseudo styles on mobile
	document.addEventListener("touchstart", function() {},false);
	
	// enable tabdrop - add dropdown when you have more tabs that can fit in one row
	if ($(window).width() > 949) {
		$('.nav-tabs.navbar-nav').tabdrop();
	}
//----------------- /Main navigation



//----------------- Tooltips
//add "tooltip-error" class
/*
$('.tooltip-error-trig').tooltip({
	template: '<div class="tooltip tooltip-error"><div class="tooltip-arrow"></div><div class="tooltip-inner"></div></div>'
})

// Tooltip title - add data-html="true" to the html tag that activates the tooltip
$('[data-toggle=tooltip]').tooltip({
	//selector: "[rel=tooltip]"
	html: true
})
*/
//----------------- /Tooltips



//----------------- Popover
$('[data-toggle=popover]:not([data-container=body])').on('shown.bs.popover', function () {
	$('.popover').css('top',parseInt($('.popover').css('top')) + 38 + 'px')
})
//----------------- /Popover



//----------------- Select field
// add span that holds the select tag
$("select:not([multiple]):not(.input-lg):not(.input-sm):not(.combobox)").each(function(){
	$(this).wrap("<span class='select-wrapper'></span>");
	$(this).after("<span class='holder'></span>");
});
$("select:not([multiple])").change(function(){
	var selectedOption = $(this).find(":selected").text();
	$(this).next(".holder").text(selectedOption);
}).trigger('change');

// large
$("select:not([multiple]).input-lg").each(function(){
	$(this).wrap("<span class='select-wrapper select-wrapper-lg'></span>");
	$(this).after("<span class='holder'></span>");
});
$("select:not([multiple]).input-lg").change(function(){
	var selectedOptionLg = $(this).find(":selected").text();
	$(this).next(".holder").text(selectedOptionLg);
}).trigger('change');

// small
$("select:not([multiple]).input-sm").each(function(){
	$(this).wrap("<span class='select-wrapper select-wrapper-sm'></span>");
	$(this).after("<span class='holder'></span>");
});
$("select:not([multiple]).input-sm").change(function(){
	var selectedOptionSm = $(this).find(":selected").text();
	$(this).next(".holder").text(selectedOptionSm);
}).trigger('change');

// Pressed
$("select:not([multiple])").focus(function(){
	$(this).parent().addClass("activeState");
}).blur(function(){
	$(this).parent().removeClass("activeState");
}).change(function(){
	$(this).parent().removeClass("activeState");
}) 
//----------------- /Select field

		
		
//----------------- Customized radio/checkbox elements - Focus state
$(".checkbox input[type='checkbox']").focus(function(){
	$(this).parent().addClass("focusState");
	}).blur(function(){
		$(this).parent().removeClass("focusState");
	})   
$(".radio input[type='radio']").focus(function(){
	$(this).parent().addClass("focusState");
	}).blur(function(){
		$(this).parent().removeClass("focusState");
	})   
//----------------- /Customized radio/checkbox elements - Focus state



//----------------- Tables
// pressed state on the head of the table
$(function(){
	$('.table-active thead th, .table-active thead td, .table-sorter thead th, .table-sorter thead td').mousedown(function(){
		$(this).addClass('pressed');
	})
	.mouseup(function(){
		$(this).removeClass('pressed');
	});
});



// pressed state on the row in the body of the table
$(function(){
	$('.table-active tbody th, .table-active tbody td, .table-sorter tbody th, .table-sorter tbody td').mousedown(function(){
		$(this).parent().addClass('pressed');
	})
	.mouseup(function(){
		$(this).parent().removeClass('pressed');
	});
});
// active on body

$(document).on("click", ".table-active tbody tr, .table-sorter tbody tr", function() { //custom made to work with angular
  	$('.table-active tbody tr, .table-sorter tbody tr').removeClass('active');
	$(this).addClass('active');
});

$('.table-active tbody tr, .table-sorter tbody tr').click(function() {
	$('.table-active tbody tr, .table-sorter tbody tr').removeClass('active');
	$(this).addClass('active');
}); 
//----------------- /Tables



//----------------- Vertical menu
// add an additional class if one of the child elemnt is active 
$('.sidenav .inner-scroll > .nav').on('activate.bs.scrollspy', function () {
	if ($('.nav .nav > li').hasClass('active')) {
		$('.sidenav .inner-scroll > .nav > li.active').addClass('active-child')
	}
	else {
		$('.sidenav .inner-scroll > .nav > li').removeClass('active-child')
	}
});

$('.sidenav .inner-scroll > .nav > li:not(.show-nav) > a').click(function() {
	$('.sidenav .inner-scroll > .nav > li').removeClass('active');
	$(this).parent().addClass('active');
	$('.sidenav .inner-scroll > .nav > li').removeClass('active-child');
	$('.sidenav .inner-scroll > .nav .nav > li').removeClass('active');
}); 

$('.sidenav .inner-scroll > .nav .nav > li > a').click(function() {
	$('.sidenav .inner-scroll > .nav .nav > li').removeClass('active');
	$(this).parent().addClass('active');
	$(this).parent().parent().parent().addClass('active-child');
});


// hide vertical menu
if ($('body.show-hide-vert-menu').length > 0) {
	var menuLeft = document.getElementById( 'cbp-spmenu-s1' ),
		contentRight = document.getElementById( 'contentPushRight' ),
		showLeftPush = document.getElementById( 'showLeftPush' ),
		body = document.body;

	showLeftPush.onclick = function() {
		classie.toggle( this, 'active' );
		classie.toggle( body, 'cbp-spmenu-push-toright' );
		classie.toggle( menuLeft, 'cbp-spmenu-open' );
		$(contentRight).toggleClass('col-md-12');
		disableOther( 'showLeftPush' );
	};
}

function disableOther( button ) {
	if( button !== 'showLeftPush' ) {
		classie.toggle( showLeftPush, 'disabled' );
	}
} 
//----------------- /Vertical menu 



//----------------- Tabs
// add pressed state class
$('.nav-tabs > li > a').on('mousedown', function(e){ 
	$(this).addClass("pressed");
	});
$('.nav-tabs > li > a').on('mouseup', function(e){ 
	$(this).removeClass("pressed");
	});

// enable tabdrop - add dropdown when you have more tabs that can fit in one row
$('.nav-tabs:not(.navbar-nav)').tabdrop(); 
//----------------- /Tabs



//----------------- Wizard
// add class on parent div width the number of childs, used for the each item width
$(".wizard > ul").each(function(){
	var count = $(this).children().size();
	$(this).parent().addClass('childs-' + count);
});

// add pressed state class
$( ".wizard > ul > li" )
.mouseup(function() {
	$( this ).removeClass("pressed");
})
.mousedown(function() {
	$( this ).addClass("pressed");
});

//----------------- /Wizard



//----------------- Attach button
$("input[type='file'].input-btn-attach").change(function () {
    if ($("input[type='file'].input-btn-attach").val() == "") {
        return;
    }
    
	// your ajax submit
	$("input[type='file'].input-btn-attach").addClass("file-attached");	
});
//----------------- /Attach button
